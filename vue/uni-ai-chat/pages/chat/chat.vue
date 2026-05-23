<template>
  <view class="container flex-row">
    <!-- 聊天历史侧边栏 -->
    <chat-sidebar 
      v-if="isWidescreen" 
      :chats="chatHistory" 
      :currentChatId="currentChatId"
      @new-chat="onNewChat"
      @chat-click="onChatClick"
      @pin-chat="onPinChat"
      @rename-chat="onRenameChat"
      @delete-chat="onDeleteChat"
    />
    
    <!-- 主聊天区域 -->
    <view class="flex-1 flex-col">
      <!-- #ifdef H5 -->
      <view v-if="isWidescreen" class="header">uni-ai-chat</view>
      <!-- #endif -->
      <text class="noData" v-if="msgList.length === 0 && chatHistory.length === 0">没有对话记录</text>
      <scroll-view :scroll-into-view="scrollIntoView" scroll-y="true" class="msg-list" :enable-flex="true">
        <uni-ai-msg ref="msg" v-for="(msg,index) in msgList" :key="index" :msg="msg" @changeAnswer="changeAnswer"
          :show-cursor="index == msgList.length - 1 && msgList.length%2 === 0 && sseIndex"
          :isLastMsg="index == msgList.length - 1" @removeMsg="removeMsg(index)"></uni-ai-msg>
        <template v-if="msgList.length%2 !== 0">
          <view v-if="requestState == -100" class="retries-box">
            <text>消息发送失败</text>
            <uni-icons @click="send" color="#d22" type="refresh-filled" class="retries-icon"></uni-icons>
          </view>
          <view class="tip-ai-ing" v-else-if="msgList.length">
            <text>正在思考中...</text>
            <!-- <view v-if="NODE_ENV == 'development' && !enableStream">
              如需提速，请开通<uni-link class="uni-link" href="https://uniapp.dcloud.net.cn/uniCloud/uni-ai-chat.html"
                text="[流式响应]"></uni-link>
            </view> -->
          </view>
        </template>
        <view @click="closeSseChannel" class="stop-responding" v-if="sseIndex"> ▣ 停止响应</view>
        <view id="last-msg-item" style="height: 1px;"></view>
      </scroll-view>

      <view class="foot-box" :style="{'padding-bottom':footBoxPaddingBottom}">
        <!-- #ifdef H5 -->
        <view class="pc-menu" v-if="isWidescreen">
          <view class="pc-trash pc-menu-item" @click="clearAllMsg" title="删除">
            <image src="@/static/remove.png" mode="heightFix"></image>
          </view>
          <view class="settings pc-menu-item" @click="setLLMmodel" title="设置">
            <uni-icons color="#555" size="20px" type="settings"></uni-icons>
          </view>
        </view>
        <!-- #endif -->
        <view class="foot-box-content">
          <view v-if="!isWidescreen" class="menu">
            <uni-icons class="menu-item" @click="clearAllMsg" type="trash" size="24" color="#888"></uni-icons>
            <uni-icons class="menu-item" @click="setLLMmodel" color="#555" size="20px"
              type="settings"></uni-icons>
            <!-- 语音按钮 -->
            <uni-icons class="menu-item" @click="toggleVoiceInput" :type="isVoiceMode ? 'mic-filled' : 'mic'" size="24" 
              :color="isVoiceMode ? '#07c160' : '#888'"></uni-icons>
          </view>
          <view class="input-wrapper">
            <!-- 文字输入模式 -->
            <view class="textarea-box" v-if="!isVoiceMode">
              <textarea v-model="content" :cursor-spacing="15" class="textarea" :auto-height="!isWidescreen"
                placeholder="请输入要发送的内容" :maxlength="-1" :adjust-position="false"
                :disable-default-padding="false" placeholder-class="input-placeholder"></textarea>
            </view>
            
            <!-- 语音输入模式 -->
            <view class="voice-box" v-else @touchstart="startRecord" @touchmove="moveRecord" @touchend="endRecord" 
              @touchcancel="cancelRecord" :class="{'recording': isRecording, 'cancel': isCancelRecord}">
              <text class="voice-text">{{ voiceText }}</text>
            </view>
          </view>
          <view class="send-btn-box" :title="(msgList.length && msgList.length%2 !== 0) ? 'ai正在回复中不能发送':''">
            <!-- #ifdef H5 -->
            <text v-if="isWidescreen" class="send-btn-tip">↵ 发送 / shift + ↵ 换行</text>
            <!-- #endif -->
            <!-- 文字模式下显示发送按钮，语音模式下显示切换按钮 -->
            <button v-if="!isVoiceMode" @click="beforeSend" :disabled="inputBoxDisabled || !content" class="send"
              type="primary">发送</button>
            <button v-else @click="toggleVoiceInput" class="send" type="default">文字</button>
          </view>
        </view>
      </view>
    </view>
  </view>
  <llm-config ref="llm-config"></llm-config>
</template>

<script>
	// 引入配置文件
	import config from '@/config.js';
	
	// 接口基础地址
	const API_BASE_URL = config.apiURL || 'https://your-api-domain.com';
	
	// 获取存储的 token 的方法
	const getAccessToken = () => {
		return uni.getStorageSync('accessToken') || '';
	};
	
export default {
		data() {
			return {
				// 使聊天窗口滚动到指定元素id的值
				scrollIntoView: "",
				// 消息列表数据
				msgList: [],
				// 聊天历史
				chatHistory: [],
				// 通讯请求状态：0 发送中，100 成功，-100 失败
				requestState: 0,
				// 输入框的消息内容
				content: "",
				// 记录流式响应次数（用于控制光标显示）
				sseIndex: 0,
				// 是否启用流式响应模式（保留变量避免模板报错）
				enableStream: true,
				// 当前屏幕是否为宽屏
				isWidescreen: false,
				// 当前使用的模型
				llmModel: false,
				// 键盘高度
				keyboardHeight: 0,
				// 当前对话ID（首次传0，后端返回后更新）
				currentChatId: '',
				// 当前对话标题（可选）
				currentTitle: '',
				// 流式请求控制
				isStreaming: false,
				abortController: null,
				// 语音相关
				isVoiceMode: false, // 是否语音输入模式
				isRecording: false, // 是否正在录音
				isCancelRecord: false, // 是否取消录音
				voiceText: '按住 说话', // 语音提示文本
				recorderManager: null, // 录音管理器
				startTime: 0, // 开始录音时间
				voicePath: '', // 录音文件路径
				touchY: 0, // 触摸Y坐标
			}
		},
		computed: {
			// 输入框是否禁用
			inputBoxDisabled() {
				// 正在流式响应时禁用
				if (this.isStreaming) {
					return true
				}
				// 如果消息列表长度为奇数（即AI未回复完成），禁用
				return !!(this.msgList.length && this.msgList.length % 2 !== 0)
			},
			// 获取当前环境
			NODE_ENV() {
				return process.env.NODE_ENV
			},
			footBoxPaddingBottom() {
				return (this.keyboardHeight || 10) + 'px'
			}
		},
		// 监听msgList变化，存储到本地缓存
		watch: {
			msgList: {
				handler(msgList) {
					uni.setStorage({
						"key": "uni-ai-msg",
						"data": msgList
					})
				},
				deep: true
			},
			llmModel(llmModel) {
				let title = 'uni-ai-chat'
				if (llmModel) {
					title += ` (${llmModel})`
				}
				// #ifdef H5
				if (this.isWidescreen) {
					const headerEl = document.querySelector('.header');
					if (headerEl) headerEl.innerText = title;
				}
				// #endif
				uni.setStorage({
					key: 'uni-ai-chat-llmModel',
					data: llmModel
				})
			}
		},
		beforeMount() {
			// #ifdef H5
			// 监听屏幕宽度变化，判断是否为宽屏
			uni.createMediaQueryObserver(this).observe({
				minWidth: 650,
			}, matches => {
				this.isWidescreen = matches;
			})
			// #endif
			
			// 初始化录音管理器
			this.initRecorderManager();
		},
async mounted() {
		// 获得历史对话记录
		this.msgList = uni.getStorageSync('uni-ai-msg') || [];
		// 获得聊天历史
		this.chatHistory = uni.getStorageSync('uni-ai-chat-history') || [];
		// 获得之前设置的llmModel
		this.llmModel = uni.getStorageSync('uni-ai-chat-llmModel');
		
		// 如果上一次对话中最后一条消息是用户发送但AI未回复，自动重发
		let length = this.msgList.length
		if (length) {
			let lastMsg = this.msgList[length - 1]
			if (!lastMsg.isAi) {
				this.send()
			}
		}
		
		// DOM渲染后滚动到最后一条消息
		this.$nextTick(() => {
			this.showLastMsg()
		})
		
		// #ifdef H5
		// 处理H5键盘事件及发送快捷键
		let adjunctKeydown = false
		const textareaDom = document.querySelector('.textarea-box textarea');
		if (textareaDom) {
			textareaDom.onkeydown = e => {
				if ([16, 17, 18, 93].includes(e.keyCode)) {
					adjunctKeydown = true;
				}
				if (e.keyCode == 13 && !adjunctKeydown) {
					e.preventDefault()
					setTimeout(() => {
						this.beforeSend();
					}, 300)
				}
			};
			textareaDom.onkeyup = e => {
				if ([16, 17, 18, 93].includes(e.keyCode)) {
					adjunctKeydown = false;
				}
			};
			
			// 可视窗口高度变化时滚动到底部（适配键盘弹起）
			let initialInnerHeight = window.innerHeight;
			if (uni.getSystemInfoSync().platform == "ios") {
				textareaDom.addEventListener('focus', () => {
					let interval = setInterval(function() {
						if (window.innerHeight !== initialInnerHeight) {
							clearInterval(interval)
							document.querySelector('.container').style.height = window.innerHeight + 'px'
							window.scrollTo(0, 0);
							this.showLastMsg()
						}
					}, 1);
				})
				textareaDom.addEventListener('blur', () => {
					document.querySelector('.container').style.height = initialInnerHeight + 'px'
				})
			} else {
				window.addEventListener('resize', (e) => {
					this.showLastMsg()
				})
			}
		}
		// #endif
		
		// #ifndef H5
		uni.onKeyboardHeightChange(e => {
			this.keyboardHeight = e.height
			this.$nextTick(() => {
				this.showLastMsg()
			})
		})
		// #endif
	},
methods: {
		// 初始化录音管理器
		initRecorderManager() {
			// #ifndef H5
			this.recorderManager = uni.getRecorderManager();
			
			// 录音开始
			this.recorderManager.onStart(() => {
				console.log('录音开始');
				this.isRecording = true;
				this.voiceText = '松开 发送';
				this.startTime = Date.now();
			});
			
			// 录音结束
			this.recorderManager.onStop((res) => {
				console.log('录音结束', res);
				this.isRecording = false;
				this.voicePath = res.tempFilePath;
				
				// 录音时长（毫秒）
				const duration = Date.now() - this.startTime;
				
				// 录音太短（小于1秒）不发送
				if (duration < 1000) {
					uni.showToast({
						title: '说话时间太短',
						icon: 'none'
					});
					return;
				}
				
				// 发送语音消息
				this.sendVoiceMessage(this.voicePath);
			});
			
			// 录音错误
			this.recorderManager.onError((err) => {
				console.error('录音错误', err);
				this.isRecording = false;
				this.voiceText = '按住 说话';
				uni.showToast({
					title: '录音失败',
					icon: 'none'
				});
			});
			// #endif
			
			// #ifdef H5
			// H5 环境下使用 Web Audio API
			this.initWebAudio();
			// #endif
		},
		
		// 更新聊天历史中的当前聊天
		updateChatInHistory() {
			// 查找当前聊天在历史中的索引
			const chatIndex = this.chatHistory.findIndex(chat => chat.id === this.currentChatId);
			
			if (chatIndex !== -1) {
				// 更新现有聊天
				this.chatHistory[chatIndex] = {
					id: this.currentChatId,
					title: this.currentTitle,
					messages: [...this.msgList],
					modificationTime: new Date().toISOString(),
					isTop: this.chatHistory[chatIndex].isTop || false
				};
			} else if (this.msgList.length > 0) {
				// 创建新聊天
				const newChat = {
					id: this.currentChatId || Date.now().toString(),
					title: this.currentTitle || '新对话',
					messages: [...this.msgList],
					modificationTime: new Date().toISOString(),
					isTop: false
				};
				this.chatHistory.push(newChat);
				// 更新当前聊天ID
				if (!this.currentChatId) {
					this.currentChatId = newChat.id;
				}
			}
			
			// 保存到本地存储
			this.saveChatHistory();
		},
		
		// 聊天历史侧边栏事件处理
		onNewChat() {
			this.msgList = [];
			this.currentChatId = '';
			this.currentTitle = '';
			this.$nextTick(() => {
				this.showLastMsg();
			});
		},
		
		onChatClick(chat) {
			this.currentChatId = chat.id;
			this.currentTitle = chat.title;
			// 这里应该从后端加载聊天历史，暂时使用模拟数据
			// 在实际应用中，需要调用API获取聊天消息
			this.msgList = chat.messages || [];
			this.$nextTick(() => {
				this.showLastMsg();
			});
		},
		
		onPinChat(chat) {
			// 置顶/取消置顶
			chat.isTop = !chat.isTop;
			this.saveChatHistory();
			this.$nextTick(() => {
				this.showLastMsg();
			});
		},
		
		onRenameChat(chat) {
			uni.showModal({
				title: '重命名聊天',
				content: '请输入新的聊天标题：',
				editable: true,
				success: (res) => {
					if (res.confirm) {
						const newTitle = res.content.trim();
						if (newTitle) {
							chat.title = newTitle;
							this.saveChatHistory();
							if (this.currentChatId === chat.id) {
								this.currentTitle = newTitle;
							}
							this.$nextTick(() => {
								this.showLastMsg();
							});
						}
					}
				}
			});
		},
		
		onDeleteChat(chat) {
			uni.showModal({
				title: '删除聊天',
				content: '确定要删除此聊天吗？此操作不可恢复。',
				success: (res) => {
					if (res.confirm) {
						// 从历史中删除
						this.chatHistory = this.chatHistory.filter(c => c.id !== chat.id);
						this.saveChatHistory();
						
						// 如果删除的是当前聊天，清空当前聊天
						if (this.currentChatId === chat.id) {
							this.msgList = [];
							this.currentChatId = '';
							this.currentTitle = '';
							this.$nextTick(() => {
								this.showLastMsg();
							});
						}
					}
				}
			});
		},
		
		// 保存聊天历史到本地存储
		saveChatHistory() {
			uni.setStorage({
				key: 'uni-ai-chat-history',
				data: this.chatHistory
			});
		},
		
		// H5 语音录音（使用 Web Audio API）
		initWebAudio() {
			// H5 环境下需要用户授权麦克风权限
			this.mediaRecorder = null;
			this.audioChunks = [];
		},
		
		// 切换语音/文字输入模式
		toggleVoiceInput() {
			this.isVoiceMode = !this.isVoiceMode;
			if (!this.isVoiceMode) {
				this.voiceText = '按住 说话';
			}
		},
			
		// 开始录音
			async startRecord(e) {
				// 如果正在流式响应，不能录音
				if (this.isStreaming) {
					uni.showToast({
						title: 'AI正在回复中',
						icon: 'none'
					});
					return;
				}
				
				// 获取触摸位置
				if (e.touches && e.touches[0]) {
					this.touchY = e.touches[0].clientY;
				}
				
				this.isCancelRecord = false;
				
				// #ifndef H5
				// 小程序/App 录音
				this.recorderManager.start({
					format: 'mp3', // 录音格式
					duration: 60000, // 最大录音时长 60秒
				});
				// #endif
				
				// #ifdef H5
				// H5 录音
				try {
					const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
					this.mediaRecorder = new MediaRecorder(stream);
					this.audioChunks = [];
					
					this.mediaRecorder.ondataavailable = (event) => {
						if (event.data.size > 0) {
							this.audioChunks.push(event.data);
						}
					};
					
					this.mediaRecorder.onstop = () => {
						const audioBlob = new Blob(this.audioChunks, { type: 'audio/mp3' });
						const audioUrl = URL.createObjectURL(audioBlob);
						this.voicePath = audioUrl;
						
						// 录音时长
						const duration = Date.now() - this.startTime;
						if (duration >= 1000) {
							this.sendVoiceMessage(audioBlob);
						} else {
							uni.showToast({
								title: '说话时间太短',
								icon: 'none'
							});
						}
						
						// 关闭音轨
						stream.getTracks().forEach(track => track.stop());
					};
					
					this.mediaRecorder.start();
					this.isRecording = true;
					this.voiceText = '松开 发送';
					this.startTime = Date.now();
				} catch (err) {
					console.error('获取麦克风失败', err);
					uni.showToast({
						title: '请授权麦克风权限',
						icon: 'none'
					});
				}
				// #endif
			},
			
			// 移动录音（判断是否取消）
			moveRecord(e) {
				if (!this.isRecording) return;
				
				// #ifdef H5
				if (e.touches && e.touches[0]) {
					const currentY = e.touches[0].clientY;
					const offsetY = this.touchY - currentY;
					// 向上滑动超过50px 取消发送
					if (offsetY > 50) {
						this.isCancelRecord = true;
						this.voiceText = '松开 取消';
					} else {
						this.isCancelRecord = false;
						this.voiceText = '松开 发送';
					}
				}
				// #endif
			},
			
			// 结束录音
			endRecord() {
				if (!this.isRecording) return;
				
				// #ifndef H5
				if (this.isCancelRecord) {
					// 取消录音
					this.recorderManager.stop();
					this.recorderManager.onStop(() => {
						// 不发送语音
						this.isRecording = false;
						this.voiceText = '按住 说话';
						this.isCancelRecord = false;
					});
				} else {
					// 停止录音，会自动触发 onStop
					this.recorderManager.stop();
				}
				// #endif
				
				// #ifdef H5
				if (this.isCancelRecord) {
					// 取消录音
					if (this.mediaRecorder && this.mediaRecorder.state === 'recording') {
						this.mediaRecorder.stop();
					}
					this.isRecording = false;
					this.voiceText = '按住 说话';
					this.isCancelRecord = false;
				} else {
					// 停止录音，会自动触发 onstop
					if (this.mediaRecorder && this.mediaRecorder.state === 'recording') {
						this.mediaRecorder.stop();
					}
				}
				// #endif
			},
			
			// 取消录音
			cancelRecord() {
				if (!this.isRecording) return;
				
				// #ifndef H5
				this.recorderManager.stop();
				// #endif
				
				// #ifdef H5
				if (this.mediaRecorder && this.mediaRecorder.state === 'recording') {
					this.mediaRecorder.stop();
				}
				// #endif
				
				this.isRecording = false;
				this.voiceText = '按住 说话';
				this.isCancelRecord = false;
			},
			
			// 发送语音消息
			async sendVoiceMessage(audioData) {
				// 显示语音消息（用户消息）
				this.msgList.push({
					isAi: false,
					content: '[语音消息]',
					isVoice: true,
					voiceUrl: typeof audioData === 'string' ? audioData : URL.createObjectURL(audioData),
					voiceDuration: Math.floor((Date.now() - this.startTime) / 1000),
					create_time: Date.now()
				});
				this.showLastMsg();
				
				// 将语音转换为文字
				this.voiceToText(audioData);
			},
			
			// 语音转文字
			async voiceToText(audioData) {
				uni.showLoading({
					title: '语音识别中...'
				});
				
				try {
					// 获取 token
					const token = getAccessToken();
					const formData = new FormData();
					formData.append('audio', audioData);
					
					const response = await fetch(`${API_BASE_URL}/ai-chat/voice-to-text`, {
						method: 'POST',
						headers: {
							'Authorization': `Bearer ${token}`
						},
						body: formData
					});
					
					const result = await response.json();
					
					// 检查响应状态
					if (result.code === 200 && result.data && result.data.text) {
						// 识别成功，获取文字内容
						const recognizedText = result.data.text;
						
						console.log('语音识别结果:', recognizedText);
						console.log('识别方法:', result.data.recognition_method);
						console.log('音频时长:', result.data.duration, '秒');
						
						// 更新最后一条消息的内容
						const lastMsg = this.msgList[this.msgList.length - 1];
						if (lastMsg && !lastMsg.isAi) {
							lastMsg.content = recognizedText;
							lastMsg.isVoice = false;
							lastMsg.voiceDuration = result.data.duration;
							lastMsg.recognitionMethod = result.data.recognition_method;
							this.$forceUpdate();
						}
						uni.hideLoading();
						// 自动发送文字消息
						await this.send();
					} else {
						uni.hideLoading();
						// 识别失败，显示错误信息
						const errorMsg = result.detail || '识别失败';
						console.error('语音识别失败:', errorMsg);
						throw new Error(errorMsg);
					}
				} catch (error) {
					uni.hideLoading();
					console.error('语音识别失败:', error);
					
					// 显示具体错误信息
					uni.showToast({
						title: error.message || '语音识别失败，请手动输入',
						icon: 'none',
						duration: 2000
					});
					
					// 如果识别失败，删除语音消息
					if (this.msgList.length && this.msgList[this.msgList.length - 1] && !this.msgList[this.msgList.length - 1].isAi) {
						this.msgList.pop();
					}
				}
			},
	
	// H5 语音录音（使用 Web Audio API）
	initWebAudio() {
		// H5 环境下需要用户授权麦克风权限
		this.mediaRecorder = null;
		this.audioChunks = [];
	},
	
	// 切换语音/文字输入模式
	toggleVoiceInput() {
		this.isVoiceMode = !this.isVoiceMode;
		if (!this.isVoiceMode) {
			this.voiceText = '按住 说话';
		}
	},
			
			// 设置模型
			setLLMmodel() {
				this.$refs['llm-config'].open(model => {
					this.llmModel = model
				})
			},
			
			// 更新最后一条消息的辅助方法
			updateLastMsg(param) {
				let length = this.msgList.length
				if (length === 0) return
				let lastMsg = this.msgList[length - 1]
				if (typeof param == 'function') {
					param(lastMsg)
				} else {
					const [data, cover = false] = arguments
					if (cover) {
						lastMsg = data
					} else {
						lastMsg = Object.assign(lastMsg, data)
					}
				}
				this.msgList.splice(length - 1, 1, lastMsg)
			},
			
			// 重新回答
			async changeAnswer() {
				if (this.isStreaming) {
					this.closeSseChannel()
				}
				this.msgList.pop()
				this.updateLastMsg({
					illegal: false
				})
				this.send()
			},
			
			// 删除消息对
			removeMsg(index) {
				if (this.msgList[index].isAi) {
					index -= 1
				}
				if (this.isStreaming && index == this.msgList.length - 2) {
					this.closeSseChannel()
				}
				this.msgList.splice(index, 2)
			},
			
			// 发送前校验
			async beforeSend() {
				if (this.inputBoxDisabled) {
					return uni.showToast({
						title: 'ai正在回复中不能发送',
						icon: 'none'
					});
				}
				if (!this.content) {
					return uni.showToast({
						title: '内容不能为空',
						icon: 'none'
					});
				}
				
// 添加用户消息
				this.msgList.push({
					isAi: false,
					content: this.content,
					create_time: Date.now()
				})
				this.showLastMsg()
this.$nextTick(() => {
					this.content = ''
				})
				// 更新聊天历史中的消息
				this.updateChatInHistory()
				this.send()
			},
			
			// 核心发送方法
			async send() {
				if (this.isStreaming) {
					this.closeSseChannel()
				}
			
				const lastMsg = this.msgList[this.msgList.length - 1];
				if (lastMsg && lastMsg.isAi && this.requestState === -100) {
					this.msgList.pop();
				}
			
				const userMsg = this.msgList[this.msgList.length - 1];
				if (!userMsg || userMsg.isAi) {
					console.warn('没有可发送的用户消息');
					return;
				}
			
				const prompt = userMsg.content.trim();
				if (!prompt) return;
			
				this.requestState = 0;
				this.isStreaming = true;
				this.sseIndex = 0;
				
				const aiMsgId = Date.now();
				this.msgList.push({
					id: aiMsgId,
					isAi: true,
					content: '',
					create_time: Date.now(),
					segments: []
				});
				this.showLastMsg();
				
				this.abortController = new AbortController();
				const token = getAccessToken();
				const model = this.llmModel || 'DeepSeekChat';
				
				try {
					const response = await fetch(`${API_BASE_URL}/ai-chat/completion/v2`, {
						method: 'POST',
						headers: {
							'Content-Type': 'application/json',
							'Authorization': `Bearer ${token}`
						},
						body: JSON.stringify({
							"config_name": model,
							"agent_name": "通用助手",
							"prompt": prompt,
							"AIChatsId": this.currentChatId || 0
						}),
						signal: this.abortController.signal
					});
					
					if (!response.ok) {
						throw new Error(`HTTP ${response.status}`);
					}
					
					if (!response.body) {
						throw new Error('响应体为空');
					}
					
					const reader = response.body.getReader();
					const decoder = new TextDecoder();
					
while (true) {
						const { value, done } = await reader.read();
						if (done) break;
						
						const chunk = decoder.decode(value, { stream: true });
						const lines = chunk.split('\n\n');
						
						for (const line of lines) {
							if (!line.trim()) continue;
							
							if (line.startsWith('message:')) {
								const jsonStr = line.replace('message:', '').trim();
								try {
									const data = JSON.parse(jsonStr);
									if (data.v && typeof data.v === 'string') {
										const aiMsg = this.msgList.find(m => m.id === aiMsgId);
										if (aiMsg) {
											// 追加到消息内容
											if (!aiMsg.segments) {
												aiMsg.segments = [];
											}
											
											// 查找最后一个message类型的segment
											const lastMessageSegment = aiMsg.segments
												.slice()
												.reverse()
												.find(s => s.type === 'message');
											
											if (lastMessageSegment) {
												lastMessageSegment.content += data.v;
											} else {
												// 创建新的message segment
												aiMsg.segments.push({
													type: 'message',
													content: data.v
												});
											}
											
											this.sseIndex++;
											this.$forceUpdate();
											this.showLastMsg();
										}
									}
								} catch (e) {
									console.error('解析message失败', e);
								}
							}
							
							if (line.startsWith('thinking:')) {
								const jsonStr = line.replace('thinking:', '').trim();
								try {
									const data = JSON.parse(jsonStr);
									if (data.v && typeof data.v === 'string') {
										const aiMsg = this.msgList.find(m => m.id === aiMsgId);
										if (aiMsg) {
											if (!aiMsg.segments) {
												aiMsg.segments = [];
											}
											
											// 查找最后一个thinking类型的segment
											const lastThinkingSegment = aiMsg.segments
												.slice()
												.reverse()
												.find(s => s.type === 'thinking');
											
											const thinkingContent = data.v;
											
											if (lastThinkingSegment) {
												lastThinkingSegment.content += thinkingContent;
											} else {
												// 创建新的thinking segment，初始折叠
												aiMsg.segments.push({
													type: 'thinking',
													content: thinkingContent,
													collapsed: true
												});
											}
											
											this.$forceUpdate();
											this.showLastMsg();
										}
									}
								} catch (e) {
									console.error('解析thinking失败', e);
								}
							}
							
							if (line.startsWith('tool_start:')) {
								const jsonStr = line.replace('tool_start:', '').trim();
								try {
									const data = JSON.parse(jsonStr);
									const aiMsg = this.msgList.find(m => m.id === aiMsgId);
									if (aiMsg) {
										if (!aiMsg.segments) {
											aiMsg.segments = [];
										}
										
										aiMsg.segments.push({
											type: 'tool',
											callId: data.callId || '',
											toolName: data.toolName || 'unknown',
											arguments: data.toolArgs || '',
											status: 'running',
											collapsed: true
										});
										
										this.$forceUpdate();
										this.showLastMsg();
									}
								} catch (e) {
									console.error('解析tool_start失败', e);
								}
							}
							
							if (line.startsWith('tool_end:')) {
								const jsonStr = line.replace('tool_end:', '').trim();
								try {
									const data = JSON.parse(jsonStr);
									const aiMsg = this.msgList.find(m => m.id === aiMsgId);
									if (aiMsg && aiMsg.segments) {
										// 查找匹配的tool segment并更新状态
										for (let i = aiMsg.segments.length - 1; i >= 0; i--) {
											const seg = aiMsg.segments[i];
											if (seg.type === 'tool' && 
												seg.callId === (data.callId || '') && 
												seg.status === 'running') {
												seg.result = data.toolResult || '';
												seg.status = 'done';
												break;
											}
										}
										
										this.$forceUpdate();
										this.showLastMsg();
									}
								} catch (e) {
									console.error('解析tool_end失败', e);
								}
							}
							
							if (line.startsWith('echarts_start:')) {
								const jsonStr = line.replace('echarts_start:', '').trim();
								try {
									const data = JSON.parse(jsonStr);
									const aiMsg = this.msgList.find(m => m.id === aiMsgId);
									if (aiMsg) {
										if (!aiMsg.segments) {
											aiMsg.segments = [];
										}
										
										let title = '图表';
										let chartType = '';
										
										// 尝试从 toolArgs 解析实际参数
										if (data.toolArgs) {
											try {
												const argsObj = JSON.parse(data.toolArgs);
												const args = argsObj?.arguments;
												if (args) {
													title = args.title || title;
													chartType = args.chartType || '';
												}
											} catch {}
										} else if (data.v) {
											// 备用从 v 解析
											try {
												const vObj = JSON.parse(data.v);
												title = vObj?.arguments?.title || vObj?.title || title;
												chartType = vObj?.arguments?.chartType || '';
											} catch {}
										}
										
										aiMsg.segments.push({
											type: 'echarts',
											status: 'loading',
											title,
											chartType,
											collapsed: false,
											callId: data.callId
										});
										
										this.$forceUpdate();
										this.showLastMsg();
									}
								} catch (e) {
									console.error('解析echarts_start失败', e);
								}
							}
							
							if (line.startsWith('echarts_end:')) {
								const jsonStr = line.replace('echarts_end:', '').trim();
								try {
									const data = JSON.parse(jsonStr);
									const aiMsg = this.msgList.find(m => m.id === aiMsgId);
									if (aiMsg && aiMsg.segments) {
										const targetCallId = data.callId;
										let option = {};
										
										// 优先使用后端直接给的 chartOption 对象
										if (data.chartOption && typeof data.chartOption === 'object') {
											option = data.chartOption;
										} else {
											// 兼容旧格式：从 toolResult 或 v 中解析
											var rawOption = (data.toolResult !== undefined) ? data.toolResult : data.v;
											if (typeof rawOption === 'string') {
												try { option = JSON.parse(rawOption); } catch (e) {}
											} else if (typeof rawOption === 'object' && rawOption !== null) {
												option = rawOption;
											}
										}
										
										var segments = aiMsg.segments || [];
										var loadingSegment = segments.find(
											function(seg) { 
												return seg.type === 'echarts' && seg.status === 'loading' && seg.callId === targetCallId; 
											}
										);
										
										if (loadingSegment) {
											loadingSegment.status = 'done';
											loadingSegment.chartOption = option;
											loadingSegment.title = (option.title && typeof option.title === 'object' && option.title.text) ? option.title.text : loadingSegment.title;
											loadingSegment.collapsed = (loadingSegment.collapsed !== undefined) ? loadingSegment.collapsed : false;
										} else {
											aiMsg.segments = segments.concat([{
												type: 'echarts',
												status: 'done',
												chartOption: option,
												title: (option.title && typeof option.title === 'object' && option.title.text) ? option.title.text : '图表',
												collapsed: false,
												callId: targetCallId,
											}]);
										}
										
										this.$forceUpdate();
										this.showLastMsg();
									}
								} catch (e) {
									console.error('解析echarts_end失败', e);
								}
							}
							
							if (line.startsWith('title:')) {
								const jsonStr = line.replace('title:', '').trim();
								try {
									const data = JSON.parse(jsonStr);
									if (data.content) {
										this.currentTitle = data.content;
									}
								} catch (e) {}
							}
							
							if (line.startsWith('session_id:')) {
								const sessionIdStr = line.replace('session_id:', '').trim();
								try {
									const sessionData = JSON.parse(sessionIdStr);
									if (sessionData.id) {
										this.currentChatId = sessionData.id;
									}
								} catch (e) {}
							}
							
							if (line.startsWith('error:')) {
								const jsonStr = line.replace('error:', '').trim();
								try {
									const data = JSON.parse(jsonStr);
									const aiMsg = this.msgList.find(m => m.id === aiMsgId);
									if (aiMsg && data.v) {
										if (!aiMsg.errorContent) {
											aiMsg.errorContent = '';
										}
										aiMsg.errorContent += data.v;
										
										this.$forceUpdate();
										this.showLastMsg();
									}
								} catch (e) {
									console.error('解析error失败', e);
								}
							}
						}
					}
					
					this.requestState = 100;
					
				} catch (error) {
					console.error('流式请求出错:', error);
					const aiMsg = this.msgList.find(m => m.id === aiMsgId);
					if (aiMsg) {
						aiMsg.content = '抱歉，服务遇到了一点问题，请稍后再试。';
						aiMsg.illegal = true;
					}
					this.requestState = -100;
					uni.showToast({
						title: '请求失败',
						icon: 'none'
					});
				} finally {
					this.isStreaming = false;
					this.sseIndex = 0;
					this.abortController = null;
					this.showLastMsg();
				}
			},
			
			// 停止响应
			closeSseChannel() {
				if (this.abortController) {
					this.abortController.abort();
					this.abortController = null;
				}
				this.isStreaming = false;
				this.sseIndex = 0;
				if (this.msgList.length && this.msgList[this.msgList.length - 1].isAi && !this.msgList[this.msgList.length - 1].content) {
					this.msgList.pop();
				}
				this.requestState = -100;
				this.showLastMsg();
			},
			
			// 滚动到最后一条消息
			showLastMsg() {
				this.$nextTick(() => {
					this.scrollIntoView = "last-msg-item"
					this.$nextTick(() => {
						this.scrollIntoView = ""
					})
				})
			},
			
			// 清空所有消息
			clearAllMsg() {
				uni.showModal({
					title: "确认要清空聊天记录？",
					content: '本操作不可撤销',
					complete: (e) => {
						if (e.confirm) {
							this.closeSseChannel();
							this.msgList.splice(0, this.msgList.length);
							this.currentChatId = 0;
						}
					}
				});
			}
		}
	}
</script>

<style lang="scss">
	/* #ifdef VUE3 && APP-PLUS */
	@import "@/components/uni-ai-msg/uni-ai-msg.scss";
	/* #endif */

	/* #ifndef APP-NVUE */
	page,
	/* #ifdef H5 */
	.container *,
	/* #endif */
	view,
	textarea,
	button {
		display: flex;
		box-sizing: border-box;
	}

	page {
		height: 100%;
		width: 100%;
	}

	/* #endif */

	.stop-responding {
		font-size: 14px;
		border-radius: 3px;
		margin-bottom: 15px;
		background-color: #f0b00a;
		color: #FFF;
		width: 90px;
		height: 30px;
		line-height: 30px;
		margin: 0 auto;
		justify-content: center;
		margin-bottom: 15px;
		/* #ifdef H5 */
		cursor: pointer;
		/* #endif */
	}

	.stop-responding:hover {
		box-shadow: 0 0 10px #aaa;
	}

	.container {
		height: 100%;
		background-color: #FAFAFA;
		flex-direction: column;
		align-items: center;
		justify-content: center;
	}

	.foot-box {
		width: 750rpx;
		display: flex;
		flex-direction: column;
		padding: 10px 0px;
		background-color: #FFF;
		border-top: 1px solid #e5e5e5;
	}

	.foot-box-content {
		flex-direction: row;
		align-items: flex-end;
		padding: 0 12px;
		gap: 8px;
	}
	
	.input-wrapper {
		flex: 1;
		min-width: 0;
		margin: 0;
	}

	.textarea-box {
		background-color: #f5f5f5;
		border-radius: 8px;
		padding: 8px 12px;
		border: 1px solid #e5e5e5;
		width: 100%;
		transition: all 0.3s;
		
		&:focus-within {
			border-color: #07c160;
			background-color: #fff;
		}
	}

	.textarea-box .textarea {
		max-height: 120px;
		font-size: 14px;
		/* #ifndef APP-NVUE */
		overflow: auto;
		/* #endif */
		width: 450rpx;
		font-size: 14px;
	}
	
	/* #ifdef H5 */
	.textarea-box .textarea {
		resize: none;
		outline: none;
		
		&::-webkit-scrollbar {
			width: 4px;
		}
		
		&::-webkit-scrollbar-track {
			background: #f1f1f1;
			border-radius: 2px;
		}
		
		&::-webkit-scrollbar-thumb {
			background: #c1c1c1;
			border-radius: 2px;
			
			&:hover {
				background: #a8a8a8;
			}
		}
	}
	/* #endif */
	
	.voice-box {
		flex: 1;
		height: 40px;
		background-color: #f5f5f5;
		border-radius: 22px;
		justify-content: center;
		align-items: center;
		padding: 0 16px;
		cursor: pointer;
		transition: all 0.3s;
		border: 1px solid #e5e5e5;
		
		&.recording {
			background-color: #e8f5e9;
			border-color: #07c160;
			transform: scale(0.98);
		}
		
		&.cancel {
			background-color: #ffebee;
			border-color: #f44336;
			
			.voice-text {
				color: #f44336;
			}
		}
		
		.voice-text {
			font-size: 15px;
			color: #666;
			text-align: center;
			flex: 1;
			font-weight: 500;
		}
	}
	
	.recording .voice-text {
		color: #07c160;
	}

	/* #ifdef H5 */
	/*隐藏滚动条*/
	.textarea-box .textarea::-webkit-scrollbar {
		width: 0;
	}

	/* #endif */

	.input-placeholder {
		color: #bbb;
		font-size: 15px;
		line-height: 1.5;
	}

	.trash,
	.send {
		width: 50px;
		height: 30px;
		justify-content: center;
		align-items: center;
		flex-shrink: 0;
		border-radius: 8px;
	}

	.trash {
		width: 36px;
		margin-left: 5px;
	}

	.menu {
		flex-direction: row;
		justify-content: center;
		align-items: center;
		flex-shrink: 0;
		gap: 12px;
	}

	.menu-item {
		width: 20px;
		height: 40px;
		justify-content: center;
		align-items: center;
	}

	.send {
		color: #FFF;
		background-color: #07c160;
		border-radius: 8px;
		display: flex;
		margin: 0;
		padding: 0 16px;
		font-size: 14px;
		font-weight: 500;
		height: 40px;
		line-height: 40px;
		min-width: 60px;
		
		&[disabled] {
			background-color: #c8c8c8;
			color: #fff;
			opacity: 0.6;
		}
		
		&:active {
			transform: scale(0.96);
		}
	}

	/* #ifndef APP-NVUE */
	.send::after {
		display: none;
	}
	/* #endif */

	.msg-list {
		height: 0;
		flex: 1;
		width: 750rpx;
	}

	.noData {
		margin-top: 15px;
		text-align: center;
		width: 750rpx;
		color: #aaa;
		font-size: 12px;
		justify-content: center;
	}
	
	.open-ad-btn-box{
		justify-content: center;
		margin: 10px 0;
	}

	.tip-ai-ing {
		align-items: center;
		flex-direction: column;
		font-size: 14px;
		color: #919396;
		padding: 15px 0;
	}

	.uni-link {
		margin-left: 5px;
		line-height: 20px;
	}

	/* #ifdef H5 */
	@media screen and (min-width:650px) {
		.foot-box {
			border-top: solid 1px #dde0e2;
			width: 100%;
		}

		.container,.container * {
			max-width: 950px;
		}

		.container {
			box-shadow: 0 0 5px #e0e1e7;
			height: calc(100vh - 44px);
			margin: 22px auto;
			border-radius: 10px;
			overflow: hidden;
			background-color: #FAFAFA;
		}
		
		page {
			background-color: #efefef;
		}

		.container .header {
			height: 44px;
			line-height: 44px;
			border-bottom: 1px solid #F0F0F0;
			width: 100vw;
			justify-content: center;
			font-weight: 500;
		}

		.content {
			background-color: #f9f9f9;
			position: relative;
			max-width: 90%;
		}

		.foot-box,
		.foot-box-content,
		.msg-list,
		.msg-item,
		.noData,
		.textarea-box,
		.textarea,
		textarea-box,
		.input-wrapper {
			width: 100% !important;
		}

		.textarea-box {
			min-height: 44px;
		}
		
		.textarea-box .textarea {
			min-height: 44px;
			font-size: 15px;
		}

		.foot-box,
		.textarea-box {
			background-color: #FFF;
		}

		.foot-box-content {
			flex-direction: row;
			justify-content: space-between;
			align-items: center;
			padding: 8px 12px;
		}

		.pc-menu {
			flex-direction: row;
			padding: 0 12px;
			gap: 12px;
		}

		.pc-menu-item {
			height: 32px;
			width: 32px;
			justify-content: center;
			align-items: center;
			display: flex;
			cursor: pointer;
			border-radius: 6px;
			transition: background-color 0.2s;
			
			&:hover {
				background-color: #f0f0f0;
			}
		}

		.pc-trash {
			opacity: 0.7;
			
			&:hover {
				opacity: 1;
			}
		}

		.pc-trash image {
			height: 18px;
			width: 18px;
		}

		.send-btn-box {
			flex-direction: row;
			align-items: center;
			gap: 8px;
		}
		
		.send-btn-box .send-btn-tip {
			color: #919396;
			margin-right: 8px;
			font-size: 12px;
			line-height: 28px;
		}
		
		.send {
			margin-right: 0;
		}
	}
	/* #endif */
	
	/* 移动端适配 */
	/* #ifndef H5 */
	@media screen and (max-width: 649px) {
		.foot-box-content {
			padding: 0 10px;
			gap: 6px;
		}
		
		.menu-item {
			width: 36px;
			height: 36px;
		}
		
		.send {
			color: #FFF;
			border-radius: 4px;
			display: flex;
			margin: 0;
			padding: 0;
			font-size: 14px;
			margin-right: 20rpx;
		}
		
		.textarea-box .textarea {
			font-size: 16px;
		}
	}
	/* #endif */
	
	.retries-box{
		justify-content: center;
		align-items: center;
		font-size: 12px;
		color: #d2071b;
		padding: 12px;
		gap: 8px;
	}
	.retries-icon{
		font-size: 18px;
		cursor: pointer;
		
		&:active {
			transform: scale(0.9);
		}
	}
</style>
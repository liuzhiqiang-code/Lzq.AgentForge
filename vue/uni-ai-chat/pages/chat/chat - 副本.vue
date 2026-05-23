<template>
	<view class="container">
		<!-- #ifdef H5 -->
		<view v-if="isWidescreen" class="header">uni-ai-chat</view>
		<!-- #endif -->
		<text class="noData" v-if="msgList.length === 0">没有对话记录</text>
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
					<text>uni-ai正在思考中...</text>
					<view v-if="NODE_ENV == 'development' && !enableStream">
						如需提速，请开通<uni-link class="uni-link" href="https://uniapp.dcloud.net.cn/uniCloud/uni-ai-chat.html"
							text="[流式响应]"></uni-link>
					</view>
				</view>
			</template>
			<view v-if="adpid" class="open-ad-btn-box">
				<text style="color: red;">
					默认不启用广告组件(被注释)，如需使用，请"去掉注释"(“重新运行”后生效)
					位置：/pages/chat/chat.vue 第30行，或全局搜索 uni-ad-rewarded-video
				</text>
			</view>
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
				</view>
				<view class="textarea-box">
					<textarea v-model="content" :cursor-spacing="15" class="textarea" :auto-height="!isWidescreen"
						placeholder="请输入要发送的内容" :maxlength="-1" :adjust-position="false"
						:disable-default-padding="false" placeholder-class="input-placeholder"></textarea>
				</view>
				<view class="send-btn-box" :title="(msgList.length && msgList.length%2 !== 0) ? 'ai正在回复中不能发送':''">
					<!-- #ifdef H5 -->
					<text v-if="isWidescreen" class="send-btn-tip">↵ 发送 / shift + ↵ 换行</text>
					<!-- #endif -->
					<button @click="beforeSend" :disabled="inputBoxDisabled || !content" class="send"
						type="primary">发送</button>
				</view>
			</view>
		</view>
		<llm-config ref="llm-config"></llm-config>
	</view>
</template>

<script>
	// 引入配置文件
	import config from '@/config.js';
	
	// 接口基础地址，请根据实际修改
	const API_BASE_URL = config.apiURL || 'https://your-api-domain.com';
	// 获取存储的 token 的方法（需根据实际存储 key 调整）
	const getAccessToken = () => {
		// 示例：从 uni 存储中获取 token，请替换为实际存储方式
		return uni.getStorageSync('accessToken') || '';
	};
	
	// 获取广告id
	const {
		adpid
	} = config
	
	export default {
		data() {
			return {
				// 使聊天窗口滚动到指定元素id的值
				scrollIntoView: "",
				// 消息列表数据
				msgList: [],
				// 通讯请求状态：0 发送中，100 成功，-100 失败
				requestState: 0,
				// 本地对话是否因积分不足而终止（保留兼容）
				insufficientScore: false,
				// 输入框的消息内容
				content: "",
				// 记录流式响应次数（用于控制光标显示）
				sseIndex: 0,
				// 是否启用流式响应模式（保留变量避免模板报错）
				enableStream: true,
				// 当前屏幕是否为宽屏
				isWidescreen: false,
				// 广告位id
				adpid,
				// 当前使用的模型
				llmModel: false,
				// 键盘高度
				keyboardHeight: 0,
				// 当前对话ID（首次传0，后端返回后更新）
				currentChatId: 0,
				// 当前对话标题（可选）
				currentTitle: '',
				// 流式请求控制
				isStreaming: false,
				abortController: null
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
			insufficientScore(insufficientScore) {
				uni.setStorage({
					"key": "uni-ai-chat-insufficientScore",
					"data": insufficientScore
				})
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
		},
		async mounted() {
			// 获得历史对话记录
			this.msgList = uni.getStorageSync('uni-ai-msg') || [];
			// 获得之前设置的llmModel
			this.llmModel = uni.getStorageSync('uni-ai-chat-llmModel');
			// 获得积分不足标记
			this.insufficientScore = uni.getStorageSync('uni-ai-chat-insufficientScore') || false;
			
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
			// 设置模型（由 llm-config 组件调用）
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
			
			// 重新回答（换一个答案）
			async changeAnswer() {
				// 如果还在流式响应中，先停止
				if (this.isStreaming) {
					this.closeSseChannel()
				}
				// 删除旧的AI回答
				this.msgList.pop()
				this.updateLastMsg({
					illegal: false
				})
				this.insufficientScore = false
				this.send()
			},
			
			// 删除消息对
			removeMsg(index) {
				// 成对删除，如果点击的是AI回答，定位到对应的用户消息
				if (this.msgList[index].isAi) {
					index -= 1
				}
				// 如果删除的是正在流式响应的消息，关闭连接
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
				this.send()
			},
			
			// 核心发送方法（对接流式接口）
			async send() {
				// 避免重复发送，如果已有流式则关闭
				if (this.isStreaming) {
					this.closeSseChannel()
				}
				
				// 重试处理：如果最后一条是失败的AI消息，先删除
				const lastMsg = this.msgList[this.msgList.length - 1];
				if (lastMsg && lastMsg.isAi && this.requestState === -100) {
					this.msgList.pop();
				}
				
				// 确保最后一条消息是用户消息
				const userMsg = this.msgList[this.msgList.length - 1];
				if (!userMsg || userMsg.isAi) {
					console.warn('没有可发送的用户消息');
					return;
				}
				
				const prompt = userMsg.content.trim();
				if (!prompt) return;
				
				// 重置状态
				this.requestState = 0;
				this.isStreaming = true;
				this.sseIndex = 0; // 开始流式，索引从0开始计数
				
				// 创建AI占位消息
				const aiMsgId = Date.now();
				this.msgList.push({
					id: aiMsgId,        // 用于内部追踪
					isAi: true,
					content: '',
					create_time: Date.now()
				});
				this.showLastMsg();
				
				// 创建 AbortController 用于取消请求
				this.abortController = new AbortController();
				const token = getAccessToken();
				const model = this.llmModel || 'DeepSeekChat'; // 默认模型
				
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
					
					// 循环读取流
					while (true) {
						const { value, done } = await reader.read();
						if (done) break;
						
						const chunk = decoder.decode(value, { stream: true });
						const lines = chunk.split('\n\n');
						
						for (const line of lines) {
							if (!line.trim()) continue;
							
							// 处理流式文本消息（格式：message: {"v": "内容"}）
							if (line.startsWith('message:')) {
								const jsonStr = line.replace('message:', '').trim();
								try {
									const data = JSON.parse(jsonStr);
									if (data.v && typeof data.v === 'string') {
										// 找到当前AI消息并追加内容
										const aiMsg = this.msgList.find(m => m.id === aiMsgId);
										if (aiMsg) {
											aiMsg.content += data.v;
											this.sseIndex++; // 每次收到内容增加计数，用于显示光标
											// 强制更新视图
											this.$forceUpdate();
											this.showLastMsg();
										}
									}
								} catch (e) {
									console.error('解析message失败', e);
								}
							}
							
							// 处理标题更新（格式：title: {"content": "新标题"}）
							if (line.startsWith('title:')) {
								const jsonStr = line.replace('title:', '').trim();
								try {
									const data = JSON.parse(jsonStr);
									if (data.content) {
										this.currentTitle = data.content;
										// 可以在这里更新页面标题或其他UI
									}
								} catch (e) {
									console.error('解析title失败', e);
								}
							}
							
							// 处理会话ID更新（如果后端返回 session_id 字段）
							if (line.startsWith('session_id:')) {
								const sessionIdStr = line.replace('session_id:', '').trim();
								try {
									const sessionData = JSON.parse(sessionIdStr);
									if (sessionData.id) {
										this.currentChatId = sessionData.id;
									}
								} catch (e) {}
							}
							
							// 处理关闭信号（close: 可忽略）
							if (line.startsWith('close:')) {
								// 正常结束
							}
						}
					}
					
					// 流正常结束
					this.requestState = 100;
					
				} catch (error) {
					console.error('流式请求出错:', error);
					// 更新AI消息为错误提示
					const aiMsg = this.msgList.find(m => m.id === aiMsgId);
					if (aiMsg) {
						aiMsg.content = '抱歉，服务遇到了一点问题，请稍后再试。';
						// 可添加错误标记
						aiMsg.illegal = true;
					}
					this.requestState = -100;
					uni.showToast({
						title: '请求失败',
						icon: 'none'
					});
				} finally {
					// 清理状态
					this.isStreaming = false;
					this.sseIndex = 0;
					this.abortController = null;
					this.showLastMsg();
				}
			},
			
			// 停止响应（取消当前流式请求）
			closeSseChannel() {
				if (this.abortController) {
					this.abortController.abort();
					this.abortController = null;
				}
				this.isStreaming = false;
				this.sseIndex = 0;
				// 可选：删除最后一条不完整的AI消息
				if (this.msgList.length && this.msgList[this.msgList.length - 1].isAi && !this.msgList[this.msgList.length - 1].content) {
					this.msgList.pop();
				}
				this.requestState = -100; // 标记为中断
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
							// 可选重置会话ID
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
		// border: 1px solid blue;
	}

	.foot-box {
		width: 750rpx;
		display: flex;
		flex-direction: column;
		padding: 10px 0px;
		background-color: #FFF;
	}

	.foot-box-content {
		justify-content: space-around;
	}

	.textarea-box {
		padding: 8px 10px;
		background-color: #f9f9f9;
		border-radius: 5px;
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
	/*隐藏滚动条*/
	.textarea-box .textarea::-webkit-scrollbar {
		width: 0;
	}

	/* #endif */

	.input-placeholder {
		color: #bbb;
		line-height: 18px;
	}

	.trash,
	.send {
		width: 50px;
		height: 30px;
		justify-content: center;
		align-items: center;
		flex-shrink: 0;
	}

	.trash {
		width: 30rpx;
		margin-left: 10rpx;
	}

	.menu {
		justify-content: center;
		align-items: center;
		flex-shrink: 0;
	}

	.menu-item {
		width: 30rpx;
		margin: 0 10rpx;
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

	/* #ifndef APP-NVUE */
	.send::after {
		display: none;
	}

	/* #endif */


	.msg-list {
		height: 0; //不可省略，先设置为0 再由flex: 1;撑开才是一个滚动容器
		flex: 1;
		width: 750rpx;
		// border: 1px solid red;
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
		// .create_time,
		.noData,
		.textarea-box,
		.textarea,
		textarea-box {
			width: 100% !important;
		}

		.textarea-box,
		.textarea,
		textarea,
		textarea-box {
			height: 120px;
		}

		.foot-box,
		.textarea-box {
			background-color: #FFF;
		}

		.foot-box-content {
			flex-direction: column;
			justify-content: center;
			align-items: flex-end;
			padding-bottom: 0;
		}

		.pc-menu {
			padding: 0 10px;
		}

		.pc-menu-item {
			height: 20px;
			justify-content: center;
			align-items: center;
			align-content: center;
			display: flex;
			margin-right: 10px;
			cursor: pointer;
		}

		.pc-trash {
			opacity: 0.8;
		}

		.pc-trash image {
			height: 15px;
		}

		.send-btn-box .send-btn-tip {
			color: #919396;
			margin-right: 8px;
			font-size: 12px;
			line-height: 28px;
		}
	}
	/* #endif */
	.retries-box{
		justify-content: center;
		align-items: center;
		font-size: 12px;
		color: #d2071b;
	}
	.retries-icon{
		margin-top: 1px;
		margin-left: 5px;
	}
</style>
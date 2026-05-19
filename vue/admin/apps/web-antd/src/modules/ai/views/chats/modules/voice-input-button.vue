<script setup lang="ts">
import { ref } from 'vue'
import { message } from 'ant-design-vue'
import { speechToText } from '#/modules/ai/api/chats'

const emit = defineEmits<{
  (e: 'text-recognized', text: string): void
}>()

const isRecording = ref(false)
const isProcessing = ref(false)
const mediaRecorder = ref<MediaRecorder | null>(null)
const audioChunks = ref<Blob[]>([])

const startRecording = async () => {
  try {
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true })
    mediaRecorder.value = new MediaRecorder(stream)
    audioChunks.value = []
    mediaRecorder.value.ondataavailable = (event) => {
      if (event.data.size > 0) audioChunks.value.push(event.data)
    }
    mediaRecorder.value.onstop = async () => {
      const audioBlob = new Blob(audioChunks.value, { type: 'audio/webm' })
      stream.getTracks().forEach((track) => track.stop())
      await uploadAudio(audioBlob)
    }
    mediaRecorder.value.start()
    isRecording.value = true
  } catch (err) {
    message.error('无法访问麦克风，请检查权限')
    console.error(err)
  }
}

const stopAndConfirm = () => {
  if (mediaRecorder.value && mediaRecorder.value.state === 'recording') {
    mediaRecorder.value.stop()
    isRecording.value = false
    isProcessing.value = true
  }
}

const uploadAudio = async (audioBlob: Blob) => {
  try {
    const result = await speechToText(audioBlob)
    console.log('语音识别结果:', result)
    emit('text-recognized', result)
  } catch (error) {
    console.error('语音上传错误', error)
    //message.error('语音识别失败')
  } finally {
    isProcessing.value = false
  }
}

const handleClick = () => {
  if (isRecording.value) {
    stopAndConfirm()
  } else {
    startRecording()
  }
}
</script>

<template>
  <button
    @click="handleClick"
    :disabled="isProcessing"
    class="w-10 h-9 rounded-xl flex items-center justify-center transition-all disabled:opacity-50"
    :style="{
      backgroundColor: isRecording ? 'var(--error-bg)' : 'var(--bg-selector)',
      border: `1px solid ${isRecording ? 'var(--error-icon)' : 'var(--border-color)'}`,
      color: 'var(--text-tertiary)',
    }"
    title="语音录入"
  >
    <!-- 默认麦克风图标 -->
    <svg v-if="!isRecording && !isProcessing" class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <path d="M12 2a3 3 0 0 0-3 3v7a3 3 0 0 0 6 0V5a3 3 0 0 0-3-3Z"/>
      <path d="M19 10v2a7 7 0 0 1-14 0v-2"/>
      <line x1="12" x2="12" y1="19" y2="22"/>
    </svg>
    <!-- 确认（勾） -->
    <svg v-else-if="isRecording" class="w-5 h-5 text-[var(--error-icon)]" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
      <polyline points="20 6 9 17 4 12"/>
    </svg>
    <!-- 处理中旋转 -->
    <svg v-else-if="isProcessing" class="w-5 h-5 animate-spin" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
      <circle cx="12" cy="12" r="10" stroke-dasharray="32" stroke-dashoffset="10" />
    </svg>
  </button>
</template>

<style scoped>
@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
.animate-spin {
  animation: spin 1s linear infinite;
}
</style>

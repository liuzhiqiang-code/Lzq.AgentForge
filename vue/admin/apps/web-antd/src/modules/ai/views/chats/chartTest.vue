<script setup lang="ts">
import { ref, computed } from 'vue'
import { message,Textarea, Button,Alert, Empty } from 'ant-design-vue'
import ChartBlock from './modules/chartBlock.vue'

const jsonInput = ref('')
const option = ref<any>(null)
const status = ref<'idle' | 'done' | 'error'>('idle')
const errorMessage = ref('')

const title = computed(() => {
  if (option.value?.title?.text) return option.value.title.text
  return '图表预览'
})

const isButtonDisabled = computed(() => !jsonInput.value.trim())

const renderChart = () => {
  errorMessage.value = ''
  const trimmed = jsonInput.value.trim()

  if (!trimmed) {
    errorMessage.value = '请输入 JSON 配置'
    status.value = 'error'
    option.value = null
    return
  }

  try {
    const parsed = JSON.parse(trimmed)
    if (typeof parsed !== 'object' || parsed === null) {
      throw new Error('JSON 必须是一个对象')
    }
    option.value = parsed
    status.value = 'done'
    message.success('图表渲染成功')
  } catch (e: any) {
    option.value = null
    status.value = 'error'
    errorMessage.value = e.message || 'JSON 解析失败'
  }
}

const handleInput = () => {
  if (status.value !== 'idle') {
    status.value = 'idle'
    errorMessage.value = ''
  }
}
</script>

<template>
  <div class="chart-tester">
    <div class="tester-input-area">
      <label class="tester-label">粘贴 ECharts JSON 配置</label>
      <Textarea
        v-model:value="jsonInput"
        :rows="12"
        placeholder="请粘贴 ECharts option JSON..."
        @input="handleInput"
      />
      <Button
        type="primary"
        :disabled="isButtonDisabled"
        @click="renderChart"
        class="tester-button"
      >
        渲染图表
      </Button>
    </div>

    <Alert
      v-if="status === 'error'"
      type="error"
      :message="errorMessage"
      show-icon
      class="tester-alert"
    />

    <div v-if="status === 'done' && option" class="tester-chart-area">
      <ChartBlock
        :option="option"
        :status="'done'"
        :title="title"
        :collapsed="false"
      />
    </div>

    <Empty
      v-if="status === 'idle'"
      description="请在文本域中输入 ECharts 配置 JSON，然后点击“渲染图表”"
      class="tester-placeholder"
    />
  </div>
</template>

<style scoped>
.chart-tester {
  max-width: 900px;
  margin: 0 auto;
  padding: 24px;
}

.tester-input-area {
  margin-bottom: 24px;
}

.tester-label {
  display: block;
  margin-bottom: 8px;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-secondary);
}

.tester-button {
  margin-top: 12px;
}

.tester-alert {
  margin-bottom: 16px;
}

.tester-placeholder {
  margin-top: 40px;
}

.tester-chart-area {
  margin-top: 16px;
}
</style>

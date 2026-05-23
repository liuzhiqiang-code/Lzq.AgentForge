<script setup lang="ts">
import { computed, watch, ref, nextTick } from 'vue'
import { EchartsUI, useEcharts } from '@vben/plugins/echarts'
import type { EchartsUIType } from '@vben/plugins/echarts'

const props = defineProps<{
  option: any;           // 可接受对象、JSON 字符串或 null
  status: string;        // 'loading' | 'done'
  title?: string;
  collapsed?: boolean;
}>()

const emit = defineEmits<{
  (e: 'toggle'): void;
}>()

const chartRef = ref<EchartsUIType>()
const { renderEcharts } = useEcharts(chartRef)

// ========== 工具函数：递归删除值为 null 或 undefined 的属性 ==========
function removeNullish(obj: any): any {
  if (obj === null || obj === undefined) return undefined
  if (typeof obj !== 'object') return obj
  if (Array.isArray(obj)) {
    return obj.map(removeNullish).filter(v => v !== undefined)
  }
  const cleaned: Record<string, any> = {}
  for (const [key, value] of Object.entries(obj)) {
    const cleanedValue = removeNullish(value)
    if (cleanedValue !== undefined) {
      cleaned[key] = cleanedValue
    }
  }
  return Object.keys(cleaned).length > 0 ? cleaned : undefined
}

// ========== 尝试将任意输入解析为 ECharts 配置对象 ==========
function parseOption(raw: any): Record<string, unknown> | null {
  if (!raw) return null
  if (typeof raw === 'string') {
    try {
      const parsed = JSON.parse(raw)
      return typeof parsed === 'object' && parsed !== null ? parsed : null
    } catch {
      return null
    }
  }
  if (typeof raw === 'object') {
    return raw as Record<string, unknown>
  }
  return null
}

// ========== 计算最终的干净配置 ==========
const cleanedOption = computed(() => {
  if (props.status !== 'done') return null
  const parsed = parseOption(props.option)
  if (!parsed) return null
  const cleaned = removeNullish(parsed)
  return (typeof cleaned === 'object' && cleaned !== null) ? cleaned : null
})

// ========== 当干净配置变化时重新渲染 ==========
watch(
  () => cleanedOption.value,
  async (newOption) => {
    if (newOption) {
      await nextTick()
      renderEcharts(newOption)
    }
  },
  { immediate: true }
)

// ========== 监听折叠状态，展开时强制 resize ==========
watch(
  () => props.collapsed,
  (nowCollapsed) => {
    if (!nowCollapsed) {
      // 展开后需要延迟到 DOM 更新完成再 resize
      nextTick(() => {
        chartRef.value?.resize()
      })
    }
  }
)

const toggle = () => {
  emit('toggle')
}
</script>

<template>
  <div class="chart-block">
    <button
      v-if="status === 'loading' || (status === 'done' && cleanedOption)"
      class="chart-toggle"
      @click="toggle"
    >
      <svg :class="['chart-chevron', !collapsed && 'rotated']" viewBox="0 0 16 16" fill="currentColor" width="12" height="12">
        <path d="M6 4l4 4-4 4" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" />
      </svg>
      <span class="chart-title">{{ title || '图表' }}</span>
      <span v-if="status === 'loading'" class="chart-loading-text">生成中...</span>
    </button>

    <div v-show="!collapsed" class="chart-content">
      <!-- 加载中骨架屏 -->
      <div v-if="status === 'loading'" class="chart-skeleton">
        <div class="skeleton-placeholder">
          <div class="pulse-bar w-3/4"></div>
          <div class="pulse-bar w-1/2"></div>
          <div class="pulse-chart">
            <svg viewBox="0 0 200 100" class="skeleton-chart-svg">
              <path d="M10,80 L40,60 L70,70 L100,30 L130,55 L160,25 L190,45" stroke="var(--border-color)"
                stroke-width="2" fill="none" class="pulse-chart-path" />
            </svg>
          </div>
        </div>
      </div>
      <!-- 实际图表（使用干净的配置） -->
      <EchartsUI v-else-if="status === 'done' && cleanedOption" ref="chartRef" class="echarts-container" />
      <!-- 错误提示 -->
      <div v-else class="chart-error">图表数据无效</div>
    </div>
  </div>
</template>

<style scoped>
.chart-block {
  margin-bottom: 4px;
}

.chart-toggle {
  display: flex;
  align-items: center;
  gap: 6px;
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 4px 0;
  width: 100%;
  color: var(--text-muted);
  font-size: 11px;
  font-weight: 600;
}

.chart-chevron {
  transition: transform 0.2s ease;
  flex-shrink: 0;
}

.chart-chevron.rotated {
  transform: rotate(90deg);
}

.chart-title {
  letter-spacing: 0.5px;
}

.chart-loading-text {
  margin-left: auto;
  font-size: 10px;
  color: var(--text-muted);
  font-style: italic;
}

.chart-content {
  margin-top: 4px;
  border-radius: 8px;
  background: var(--bg-hover);
  padding: 8px;
}

.chart-skeleton {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 200px;
}

.skeleton-placeholder {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.pulse-bar {
  height: 10px;
  background: var(--border-color);
  border-radius: 4px;
  animation: pulse 1.5s ease-in-out infinite;
}

.pulse-chart {
  width: 100%;
  height: 100px;
}

.skeleton-chart-svg {
  width: 100%;
  height: 100%;
}

.pulse-chart-path {
  stroke-dasharray: 1000;
  stroke-dashoffset: 1000;
  animation: draw 2s ease forwards infinite;
}

@keyframes pulse {
  0% {
    opacity: 0.5;
  }

  50% {
    opacity: 1;
  }

  100% {
    opacity: 0.5;
  }
}

@keyframes draw {
  0% {
    stroke-dashoffset: 1000;
  }

  50% {
    stroke-dashoffset: 0;
  }

  100% {
    stroke-dashoffset: -1000;
  }
}

.echarts-container {
  width: 100%;
  height: 300px;
}

.chart-error {
  color: var(--error-text);
  font-size: 13px;
  padding: 8px;
}
</style>

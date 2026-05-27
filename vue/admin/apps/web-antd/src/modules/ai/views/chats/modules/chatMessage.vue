<script setup lang="ts">
import MarkdownIt from 'markdown-it'
import type { ExtendedMessage } from '../types'
import ChartBlock from './chartBlock.vue'

defineProps<{
  msg: ExtendedMessage
  selectedModelDisplay: string
}>()

const toggleCollapsed = (obj: any) => {
  obj.collapsed = !obj.collapsed
}

const formatToolJson = (raw: string) => {
  try {
    const parsed = JSON.parse(raw)
    return JSON.stringify(parsed, null, 2)
  } catch {
    return raw
  }
}

const md = new MarkdownIt({
  html: false,
  linkify: true,
  typographer: true,
})
</script>

<template>
  <div :class="['flex w-full mb-8 animate-fade-in', msg.role === 'user' ? 'justify-end' : 'justify-start']">
    <div :class="['flex max-w-[85%] items-start gap-4', msg.role === 'user' ? 'flex-row-reverse' : 'flex-row']">
      
      <!-- AI 头像 -->
      <div v-if="msg.role === 'assistant'"
        class="w-9 h-9 rounded-full flex-shrink-0 flex items-center justify-center border-2"
        style="background-color: var(--bg-avatar); border-color: var(--accent)">
        <svg class="w-5 h-5" style="color: var(--accent)" viewBox="0 0 24 24" fill="none" stroke="currentColor"
          stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M12 8V4H8"></path>
          <rect width="16" height="12" x="4" y="8" rx="2"></rect>
          <circle cx="9" cy="13" r="1"></circle>
          <circle cx="15" cy="13" r="1"></circle>
        </svg>
      </div>

      <!-- 气泡内容 -->
      <div :class="['flex flex-col min-w-0', msg.role === 'user' ? 'items-end' : 'items-start']">
        <div
          :class="['text-[14px] leading-[1.7] py-3 px-5 transition-all relative', msg.role === 'user' ? 'rounded-2xl rounded-tr-none shadow-lg' : 'rounded-2xl rounded-tl-none border']"
          :style="msg.role === 'user'
            ? { backgroundColor: 'var(--user-msg-bg)', color: 'var(--user-msg-text)', boxShadow: '0 10px 15px -3px var(--user-msg-shadow)' }
            : { backgroundColor: 'var(--bg-bubble-ai)', borderColor: 'var(--border-color)', color: 'var(--text-bubble-ai)', boxShadow: '0 2px 6px rgba(0,0,0,0.1)' }">

          <!-- AI 消息渲染 -->
          <div v-if="msg.role === 'assistant'" class="w-full">
            
            <!-- 时间线段 -->
            <template v-for="(seg, idx) in (msg.segments || [])" :key="idx">
              
              <!-- 思考 -->
              <div v-if="seg.type === 'thinking' && seg.content" class="thinking-block">
                <button class="thinking-toggle" @click="toggleCollapsed(seg)">
                  <svg :class="['thinking-chevron', !seg.collapsed && 'rotated']" viewBox="0 0 16 16" fill="currentColor" width="12" height="12">
                    <path d="M6 4l4 4-4 4" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round"/>
                  </svg>
                  <span class="thinking-label">深度思考</span>
                </button>
                <div v-show="!seg.collapsed" class="thinking-content">{{ seg.content }}</div>
              </div>

              <!-- 工具调用 -->
              <div v-else-if="seg.type === 'tool'" class="tool-block" :class="seg.status">
                <button class="tool-toggle" @click="toggleCollapsed(seg)">
                  <svg :class="['tool-chevron', !seg.collapsed && 'rotated']" viewBox="0 0 16 16" fill="currentColor" width="10" height="10">
                    <path d="M6 4l4 4-4 4" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round"/>
                  </svg>
                  <svg class="tool-icon" viewBox="0 0 16 16" fill="currentColor" width="12" height="12">
                    <path d="M4 1.5H3a2 2 0 0 0-2 2V14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V3.5a2 2 0 0 0-2-2h-1v1h1a1 1 0 0 1 1 1V14a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1V3.5a1 1 0 0 1 1-1h1v-1z"/>
                    <path d="M9.5 1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-3a.5.5 0 0 1-.5-.5v-1a.5.5 0 0 1 .5-.5h3zm-3-1A1.5 1.5 0 0 0 5 1.5v1A1.5 1.5 0 0 0 6.5 4h3A1.5 1.5 0 0 0 11 2.5v-1A1.5 1.5 0 0 0 9.5 0h-3z"/>
                  </svg>
                  <span class="tool-name">{{ seg.toolName }}</span>
                  <span v-if="seg.status === 'running'" class="tool-spinner"></span>
                  <svg v-else-if="seg.status === 'done'" class="tool-done" viewBox="0 0 16 16" fill="currentColor" width="12" height="12">
                    <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z"/>
                    <path d="M10.97 4.97a.235.235 0 0 0-.02.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-1.071-1.05z"/>
                  </svg>
                </button>
                <div v-show="!seg.collapsed" class="tool-detail">
                  <div v-if="seg.arguments" class="tool-section">
                    <span class="tool-section-label">参数</span>
                    <pre class="tool-pre">{{ formatToolJson(seg.arguments) }}</pre>
                  </div>
                  <div v-if="seg.result" class="tool-section">
                    <span class="tool-section-label">结果</span>
                    <pre class="tool-pre">{{ formatToolJson(seg.result) }}</pre>
                  </div>
                </div>
              </div>

                <!-- ========== 图表显示 ========== -->
              <div v-else-if="seg.type === 'echarts' && (seg.status === 'loading' || seg.chartOption)" 
                class="chart-wrapper">
                <ChartBlock
                  :option="seg.chartOption"
                  :status="seg.status"
                  :title="seg.title"
                  :collapsed="seg.collapsed"
                  @toggle="toggleCollapsed(seg)"
                />
              </div>

              <!-- 正文 -->
              <div v-else-if="seg.type === 'message' && seg.content" class="markdown-body" v-html="md.render(seg.content)"></div>
            </template>

            <!-- 加载状态 -->
            <span v-if="!msg.errorContent && (!msg.segments || msg.segments.length === 0)" class="loading-dots">...</span>

            <!-- 错误内容 -->
            <div v-if="msg.errorContent" class="error-block">
              <svg class="error-icon" viewBox="0 0 24 24" fill="currentColor" width="16" height="16">
                <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"/>
              </svg>
              <span>{{ msg.errorContent }}</span>
            </div>
          </div>

          <!-- 用户消息 -->
          <div v-else class="whitespace-pre-wrap break-words">
            {{ msg.content }}
          </div>
        </div>

        <!-- 模型名称 -->
        <div v-if="msg.role === 'assistant'" class="mt-2 px-1">
          <span class="text-[9px] font-bold uppercase tracking-widest" style="color: var(--text-muted)">
            {{ selectedModelDisplay }}
          </span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* 动画 */
.animate-fade-in {
  animation: fadeIn 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Markdown */
.markdown-body {
  word-break: break-word;
  line-height: 1.75;
  color: inherit;
}
.markdown-body :deep(> *:last-child) { margin-bottom: 0 !important; }
.markdown-body :deep(h1), .markdown-body :deep(h2), .markdown-body :deep(h3) {
  color: inherit; font-weight: 600; margin-top: 1.5rem; margin-bottom: 0.75rem; line-height: 1.3;
}
.markdown-body :deep(h1) { font-size: 1.5rem; border-bottom: 1px solid var(--border-color); padding-bottom: 0.3rem; }
.markdown-body :deep(h2) { font-size: 1.25rem; }
.markdown-body :deep(h3) { font-size: 1.1rem; }
.markdown-body :deep(p) { margin-bottom: 1rem; }
.markdown-body :deep(ul), .markdown-body :deep(ol) { padding-left: 1.5rem; margin-bottom: 1rem; }
.markdown-body :deep(li) { margin-bottom: 0.25rem; }
.markdown-body :deep(blockquote) {
  margin: 1rem 0; padding: 0 1rem; color: var(--text-muted);
  border-left: 0.25rem solid var(--accent); background: rgba(9, 96, 189, 0.05);
}
.markdown-body :deep(code:not(pre code)) {
  font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
  background-color: var(--bg-hover); padding: 0.2rem 0.4rem; border-radius: 6px; font-size: 88%; color: inherit;
}
.markdown-body :deep(pre) {
  background-color: var(--bg-secondary); padding: 1rem; border-radius: 12px;
  overflow-x: auto; margin: 1.2rem 0; border: 1px solid var(--border-color);
}
.markdown-body :deep(pre code) {
  background-color: transparent; padding: 0; font-size: 13px; line-height: 1.5;
  color: inherit; font-family: 'Fira Code', 'Cascadia Code', monospace;
}
.markdown-body :deep(table) { width: 100%; border-collapse: collapse; margin-bottom: 1rem; font-size: 13px; }
.markdown-body :deep(th), .markdown-body :deep(td) { padding: 8px 12px; border: 1px solid var(--border-color); }
.markdown-body :deep(th) { background-color: var(--bg-hover); text-align: left; }
.markdown-body :deep(a) { color: var(--accent); text-decoration: none; }

/* 思考区 */
.thinking-block { margin-bottom: 4px; }
.thinking-toggle {
  display: flex; align-items: center; gap: 6px; background: transparent;
  border: none; cursor: pointer; padding: 4px 0; width: 100%; transition: opacity 0.2s;
}
.thinking-toggle:hover { opacity: 0.8; }
.thinking-chevron { transition: transform 0.2s ease; color: var(--text-muted); flex-shrink: 0; }
.thinking-chevron.rotated { transform: rotate(90deg); }
.thinking-label { font-size: 11px; font-weight: 600; color: var(--text-muted); letter-spacing: 0.5px; text-transform: uppercase; }
.thinking-content {
  font-size: 11px; color: var(--text-muted); line-height: 1.6; padding: 8px 12px; margin-top: 2px;
  background: var(--bg-hover); border-radius: 8px; border-left: 2px solid var(--accent);
  white-space: pre-wrap; word-break: break-word;
}
.thinking-content, .tool-detail {
  transition: opacity 0.2s, transform 0.2s;
}
.thinking-content[v-show="false"], .tool-detail[v-show="false"] {
  display: none;
}

/* 工具调用区 */
.tool-block { margin-bottom: 4px; }
.tool-toggle {
  display: flex; align-items: center; gap: 6px; background: transparent;
  border: none; cursor: pointer; padding: 4px 0; width: 100%; transition: opacity 0.2s;
}
.tool-toggle:hover { opacity: 0.8; }
.tool-chevron { transition: transform 0.2s ease; color: var(--text-muted); flex-shrink: 0; }
.tool-chevron.rotated { transform: rotate(90deg); }
.tool-icon { color: var(--accent); flex-shrink: 0; }
.tool-name { font-size: 11px; font-weight: 600; color: var(--text-muted); letter-spacing: 0.3px; }
.tool-spinner {
  width: 10px; height: 10px; border: 1.5px solid var(--text-muted);
  border-top-color: var(--accent); border-radius: 50%; animation: spin 0.8s linear infinite; flex-shrink: 0;
}
.tool-done { color: #52c41a; flex-shrink: 0; }
.tool-detail {
  margin-top: 2px; padding: 8px 10px; background: var(--bg-hover);
  border-radius: 8px; border-left: 2px solid var(--accent);
}
.tool-section { margin-bottom: 6px; }
.tool-section:last-child { margin-bottom: 0; }
.tool-section-label {
  font-size: 10px; font-weight: 700; color: var(--text-muted); text-transform: uppercase;
  letter-spacing: 0.5px; display: block; margin-bottom: 4px;
}
.tool-pre {
  font-size: 10px; font-family: 'Fira Code', 'Cascadia Code', monospace; color: var(--text-secondary);
  background: var(--bg-secondary); padding: 6px 8px; border-radius: 6px;
  overflow-x: auto; max-height: 160px; line-height: 1.5; white-space: pre-wrap; word-break: break-all;
}
@keyframes spin { to { transform: rotate(360deg); } }

/* 错误 */
.error-block {
  display: flex; align-items: flex-start; gap: 8px; margin-top: 12px; padding: 10px 12px;
  background: var(--error-bg); border: 1px solid var(--error-border); border-radius: 8px;
  color: var(--error-text); font-size: 13px; line-height: 1.5;
}
.error-icon { flex-shrink: 0; margin-top: 2px; color: var(--error-icon); }

.chart-wrapper {
  overflow-x: auto;
  width: 100%;
  min-width: 300px;   /* 保证最小宽度 */
  max-width: 100%;    /* 突破气泡的 85% 限制 */
}

.loading-dots {
  animation: dots 1.5s steps(4, end) infinite;
  overflow: hidden;
  display: inline-block;
  vertical-align: bottom;
}
@keyframes dots {
  0% { width: 0; }
  25% { width: 0.6em; }
  50% { width: 1.2em; }
  75% { width: 1.8em; }
  100% { width: 1.8em; }
}

.tool-block.running .tool-detail {
  border-left-color: var(--accent);
  background: rgba(9, 96, 189, 0.05);
}

</style>

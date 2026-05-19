<script lang="ts" setup>
import { Card, Tag, Collapse, Button } from 'ant-design-vue'
import { MdiCodeBraces, MdiFileDocumentOutline } from '@vben/icons'
import type { AgentSkillApi } from '#/modules/ai/api/agentSkill'
import { $t } from '#/locales'

defineProps<{
  skills: AgentSkillApi.SkillItem[]
}>()

const emit = defineEmits<{
  (e: 'execute', skillName: string, tool: AgentSkillApi.ToolItem): void
}>()
</script>

<template>
  <div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
    <Card
      v-for="skill in skills"
      :key="skill.skillName"
      :bordered="true"
      class="shadow-sm"
    >
      <!-- 自定义标题区域，加入图标 -->
      <template #title>
        <div class="flex items-center gap-2">
          <MdiCodeBraces v-if="skill.type !== 'External'" class="size-5 text-blue-500" />
          <MdiFileDocumentOutline v-else class="size-5 text-orange-500" />
          <span>{{ skill.skillName }}</span>
        </div>
      </template>

      <template #extra>
        <Tag v-if="skill.type === 'External'" color="orange">外部</Tag>
        <Tag v-else color="blue">
          {{ $t('ai.agentSkill.toolsCount', { count: skill.tools.length }) }}
        </Tag>
      </template>

      <p class="mb-3 text-gray-600 dark:text-gray-300">
        {{ skill.skillDescription }}
      </p>

      <!-- 仅内部技能展示工具列表 -->
      <Collapse v-if="skill.type !== 'External'" accordion :bordered="false">
        <Collapse.Panel key="tools" :header="$t('ai.agentSkill.viewTools')">
          <div
            v-if="skill.tools.length === 0"
            class="text-gray-400 dark:text-gray-500"
          >
            {{ $t('ai.agentSkill.noTools') }}
          </div>
          <div
            v-for="tool in skill.tools"
            :key="tool.toolName"
            class="mb-3 rounded border border-gray-200 bg-gray-50 p-3 dark:border-gray-600 dark:bg-gray-800"
          >
            <div class="flex items-start justify-between gap-2">
              <div class="flex-1 min-w-0">
                <div class="font-medium text-blue-600 dark:text-blue-400">
                  {{ tool.toolName }}
                </div>
                <div class="mt-1 text-sm text-gray-500 dark:text-gray-300">
                  {{ tool.description }}
                </div>
                <div v-if="tool.parameters.length" class="mt-2">
                  <span class="text-xs text-gray-400 dark:text-gray-400">
                    {{ $t('ai.agentSkill.parameter') }}：
                  </span>
                  <div
                    v-for="param in tool.parameters"
                    :key="param.name"
                    class="ml-2 text-xs"
                  >
                    <Tag color="green">{{ param.name }}</Tag>
                    <span class="break-words text-gray-400 dark:text-gray-400">
                      {{ param.parameterType }} - {{ param.description }}
                    </span>
                  </div>
                </div>
              </div>
              <Button
                type="primary"
                size="small"
                class="flex-shrink-0 ml-2"
                @click="emit('execute', skill.skillName, tool)"
              >
                {{ $t('ai.agentSkill.executeTest') }}
              </Button>
            </div>
          </div>
        </Collapse.Panel>
      </Collapse>
    </Card>
  </div>
</template>

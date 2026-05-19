<script lang="ts" setup>
import { ref, watch } from 'vue'
import { Checkbox, Collapse, Empty, Spin, Tag } from 'ant-design-vue'
import { getAgentSkillList, type AgentSkillApi } from '#/modules/ai/api/agentSkill'
import type { AgentManageApi } from '#/modules/ai/api/agentManage'

const props = defineProps<{
  modelValue?: AgentManageApi.SkillMethodEntry[]
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: AgentManageApi.SkillMethodEntry[]): void
}>()

const skills = ref<AgentSkillApi.SkillItem[]>([])
const loadingSkills = ref(false)

async function loadSkills() {
  loadingSkills.value = true
  try {
    skills.value = await getAgentSkillList()
  } catch {
    // 忽略错误
  } finally {
    loadingSkills.value = false
  }
}
loadSkills()

// 内部状态：仅存储已选技能名称列表
const selectedSkillNames = ref<string[]>(
  (props.modelValue || []).map(s => s.skillName)
)

watch(() => props.modelValue, (val) => {
  selectedSkillNames.value = (val || []).map(s => s.skillName)
})

function emitUpdate() {
  emit('update:modelValue', selectedSkillNames.value.map(name => ({ skillName: name })))
}

function toggleSkill(skillName: string, checked: boolean) {
  if (checked) {
    if (!selectedSkillNames.value.includes(skillName)) {
      selectedSkillNames.value = [...selectedSkillNames.value, skillName]
    }
  } else {
    selectedSkillNames.value = selectedSkillNames.value.filter(n => n !== skillName)
  }
  emitUpdate()
}
</script>

<template>
  <div>
    <Spin :spinning="loadingSkills">
      <Empty v-if="skills.length === 0" description="暂无可用技能" />
      <Collapse v-else accordion :bordered="false" class="bg-transparent">
        <Collapse.Panel v-for="skill in skills" :key="skill.skillName" :header="skill.skillName">
          <template #extra>
            <Checkbox
              :checked="selectedSkillNames.includes(skill.skillName)"
              @change="(e: any) => toggleSkill(skill.skillName, e.target.checked)"
            />
          </template>
          <div class="text-xs text-gray-500 mb-2">{{ skill.skillDescription }}</div>
          <!-- 工具列表仅作展示，不再提供勾选 -->
          <div v-if="skill.tools?.length" class="ml-4 mb-1">
            <div class="text-xs text-gray-400 mb-1">包含工具：</div>
            <div v-for="tool in skill.tools" :key="tool.toolName" class="mb-1">
              <span class="font-medium text-xs">{{ tool.toolName }}</span>
              <p class="text-xs text-gray-400">{{ tool.description }}</p>
              <Tag v-for="param in tool.parameters" :key="param.name" color="blue" class="mr-1" size="small">
                {{ param.name }}
              </Tag>
            </div>
          </div>
        </Collapse.Panel>
      </Collapse>
    </Spin>
  </div>
</template>

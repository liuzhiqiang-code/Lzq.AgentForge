<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useVbenModal } from '@vben/common-ui';
import { Button } from 'ant-design-vue';
import { useVbenForm } from '#/adapter/form';
import { $t } from '#/locales';
import SkillSelector from './skillSelector.vue';

import type { AgentManageApi } from '#/modules/ai/api/agentManage';
import { createAgentManage, updateAgentManage } from '#/modules/ai/api/agentManage';

import { useSchema } from '../data';


const emit = defineEmits(['success']);
const formData = ref<AgentManageApi.AgentManage>();
const getTitle = computed(() => {
  return formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('ai.agentManage.name')])
    : $t('ui.actionTitle.create', [$t('ai.agentManage.name')]);
});

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: useSchema(),
  showDefaultActions: false,
});

function resetForm() {
  formApi.resetForm();
  formApi.setValues(formData.value || {});
}

const [Modal, modalApi] = useVbenModal({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (valid) {
      modalApi.lock();
      const data = await formApi.getValues();
      try {
        await (formData.value?.id
          ? updateAgentManage({ ...data, id: formData.value.id } as AgentManageApi.AgentManage)
          : createAgentManage(data as AgentManageApi.AgentManage));
        modalApi.close();
        emit('success');
      } finally {
        modalApi.lock(false);
      }
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = modalApi.getData<AgentManageApi.AgentManage>();
      if (data) {
        if (data.pid === 0) {
          data.pid = undefined;
        }
        formData.value = data;
        formApi.setValues(formData.value);
      }
    }
  },
});
</script>

<template>
  <Modal :title="getTitle">
    <Form class="mx-4">
      <!-- 技能绑定插件，利用 v-bind="slotProps" 将表单字段的值/更新函数传给组件 -->
      <template #selectedSkills="slotProps">
        <SkillSelector
          :model-value="slotProps.modelValue"
          @update:model-value="(val:any) => slotProps['onUpdate:modelValue'](val)"
        />
      </template>
    </Form>
    <template #prepend-footer>
      <div class="flex-auto">
        <Button type="primary" danger @click="resetForm">
          {{ $t('common.reset') }}
        </Button>
      </div>
    </template>
  </Modal>
</template>

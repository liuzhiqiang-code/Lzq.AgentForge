<script lang="ts" setup>
import type { WorkOrderApi } from '#/modules/mes/api/workorder';
import { createWorkOrder, updateWorkOrder } from '#/modules/mes/api/workorder';
import { getProcessListByLine } from '#/modules/mes/api/process';
import { computed, ref, watch } from 'vue';
import { useVbenModal } from '@vben/common-ui';
import { Button } from 'ant-design-vue';
import { useVbenForm } from '#/adapter/form';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<WorkOrderApi.WorkOrderItem>();

const getTitle = computed(() => {
  return formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('mes.workorder.workorder.name')])
    : $t('ui.actionTitle.create', [$t('mes.workorder.workorder.name')]);
});

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: useSchema(),
  showDefaultActions: false,
  wrapperClass: 'grid grid-cols-2 gap-4',
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
          ? updateWorkOrder({ ...data, id: formData.value.id } as WorkOrderApi.WorkOrderItem)
          : createWorkOrder(data as WorkOrderApi.WorkOrderItem));
        modalApi.close();
        emit('success');
      } finally {
        modalApi.lock(false);
      }
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = modalApi.getData<WorkOrderApi.WorkOrderItem>();
      formData.value = data || {};
      formApi.setValues(data || {});
    }
  },
});

/**
 * 当产线变更时加载工序列表（级联场景，使用 formApi.updateSchema 是合适的）
 */
watch(
  () => formApi.getValues()?.lineId,
  async (newLineId) => {
    if (newLineId) {
      try {
        const res = await getProcessListByLine(newLineId);
        const options = (res || []).map((item: any) => ({
          label: item.name,
          value: item.id,
        }));
        await formApi.updateSchema([
          {
            fieldName: 'processId',
            componentProps: {
              options,
              placeholder: $t('mes.workorder.workorder.processPlaceholder'),
            },
          },
        ]);
      } catch (error) {
        console.error('Failed to load process options:', error);
      }
    } else {
      await formApi.updateSchema([
        {
          fieldName: 'processId',
          componentProps: {
            options: [],
            placeholder: $t('mes.workorder.workorder.processPlaceholder'),
          },
        },
      ]);
    }
  },
);
</script>

<template>
  <Modal :title="getTitle">
    <Form class="mx-4" />
    <template #prepend-footer>
      <div class="flex-auto">
        <Button type="primary" danger @click="resetForm">
          {{ $t('common.reset') }}
        </Button>
      </div>
    </template>
  </Modal>
</template>

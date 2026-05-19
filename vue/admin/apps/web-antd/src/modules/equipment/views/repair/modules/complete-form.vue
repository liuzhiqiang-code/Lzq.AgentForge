<script lang="ts" setup>
import type { RepairApi } from '#/modules/equipment/api/repair';
import { completeRepair } from '#/modules/equipment/api/repair';
import { computed, ref } from 'vue';
import { useVbenModal } from '@vben/common-ui';
import { Button } from 'ant-design-vue';
import { useVbenForm } from '#/adapter/form';
import { $t } from '#/locales';

const emit = defineEmits(['success']);
const formData = ref<RepairApi.RepairItem>();

const getTitle = computed(() => {
  return $t('equipment.repair.complete') + ' - ' + (formData.value?.code || '');
});

const completeSchema = [
  {
    component: 'Textarea',
    fieldName: 'faultReason',
    label: $t('equipment.repair.faultReason'),
  },
  {
    component: 'Textarea',
    fieldName: 'repairProcess',
    label: $t('equipment.repair.repairProcess'),
  },
  {
    component: 'Textarea',
    fieldName: 'partsUsed',
    label: $t('equipment.repair.partsUsed'),
  },
  {
    component: 'InputNumber',
    fieldName: 'workHours',
    label: $t('equipment.repair.workHours'),
    defaultValue: 0,
  },
  {
    component: 'InputNumber',
    fieldName: 'cost',
    label: $t('equipment.repair.cost'),
    defaultValue: 0,
  },
];

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: completeSchema,
  showDefaultActions: false,
});

const [Modal, modalApi] = useVbenModal({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (valid) {
      modalApi.lock();
      const data = await formApi.getValues();
      try {
        await completeRepair({
          id: formData.value!.id!,
          ...data,
        });
        modalApi.close();
        emit('success');
      } finally {
        modalApi.lock(false);
      }
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = modalApi.getData<RepairApi.RepairItem>();
      formData.value = data;
      formApi.resetForm();
      formApi.setValues({});
    }
  },
});
</script>

<template>
  <Modal :title="getTitle">
    <Form class="mx-4" />
    <template #prepend-footer>
      <div class="flex-auto">
        <Button type="primary" danger @click="formApi.resetForm()">
          {{ $t('common.reset') }}
        </Button>
      </div>
    </template>
  </Modal>
</template>

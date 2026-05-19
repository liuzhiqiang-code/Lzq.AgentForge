<script lang="ts" setup>
import type { RepairApi } from '#/modules/equipment/api/repair';
import { assignRepair } from '#/modules/equipment/api/repair';
import { computed, ref } from 'vue';
import { useVbenModal } from '@vben/common-ui';
import { Button } from 'ant-design-vue';
import { useVbenForm } from '#/adapter/form';
import { $t } from '#/locales';

const emit = defineEmits(['success']);
const formData = ref<RepairApi.RepairItem>();

const getTitle = computed(() => {
  return $t('equipment.repair.assign') + ' - ' + (formData.value?.code || '');
});

const assignSchema = [
  {
    component: 'InputNumber',
    componentProps: { placeholder: '请输入维修人ID' },
    fieldName: 'repairUserId',
    label: '维修人ID',
    rules: 'required',
  },
  {
    component: 'Input',
    componentProps: { placeholder: '请输入维修人姓名' },
    fieldName: 'repairUserName',
    label: '维修人姓名',
  },
];

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: assignSchema,
  showDefaultActions: false,
});

const [Modal, modalApi] = useVbenModal({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (valid) {
      modalApi.lock();
      const data = await formApi.getValues();
      try {
        await assignRepair({
          id: formData.value!.id!,
          repairUserId: data.repairUserId,
          repairUserName: data.repairUserName,
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

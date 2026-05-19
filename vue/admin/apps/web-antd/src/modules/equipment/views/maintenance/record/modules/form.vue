<script lang="ts" setup>
import type { MaintenanceRecordApi } from '#/modules/equipment/api/maintenance';
import { createMaintenanceRecord } from '#/modules/equipment/api/maintenance';
import { computed, ref } from 'vue';
import { useVbenModal } from '@vben/common-ui';
import { Button } from 'ant-design-vue';
import { useVbenForm } from '#/adapter/form';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<MaintenanceRecordApi.MaintenanceRecordItem>();

const getTitle = computed(() => {
  return formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('equipment.maintenanceRecord.name')])
    : $t('ui.actionTitle.create', [$t('equipment.maintenanceRecord.name')]);
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
        await createMaintenanceRecord(data as MaintenanceRecordApi.MaintenanceRecordItem);
        // await (formData.value?.id
        //   ? updateMaintenanceRecord({ ...data, id: formData.value.id } as MaintenanceRecordApi.MaintenanceRecordItem)
        //   : createMaintenanceRecord(data as MaintenanceRecordApi.MaintenanceRecordItem));
        modalApi.close();
        emit('success');
      } finally {
        modalApi.lock(false);
      }
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = modalApi.getData<MaintenanceRecordApi.MaintenanceRecordItem>();
      formData.value = data || {};
      formApi.setValues(data || {});
    }
  },
});
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

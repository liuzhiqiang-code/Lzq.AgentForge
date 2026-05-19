<script lang="ts" setup>
import type { MaintenancePlanApi } from '#/modules/equipment/api/maintenance';
import { createMaintenancePlan } from '#/modules/equipment/api/maintenance';
import { computed, ref } from 'vue';
import { useVbenModal } from '@vben/common-ui';
import { Button } from 'ant-design-vue';
import { useVbenForm } from '#/adapter/form';
import { $t } from '#/locales';

import { useSchema } from '../data';

const emit = defineEmits(['success']);
const formData = ref<MaintenancePlanApi.MaintenancePlanItem>();

const getTitle = computed(() => {
  return formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('equipment.maintenancePlan.name')])
    : $t('ui.actionTitle.create', [$t('equipment.maintenancePlan.name')]);
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
        await createMaintenancePlan(data as MaintenancePlanApi.MaintenancePlanItem);
        // await (formData.value?.id
        //   ? updateMaintenancePlan({ ...data, id: formData.value.id } as MaintenancePlanApi.MaintenancePlanItem)
        //   : createMaintenancePlan(data as MaintenancePlanApi.MaintenancePlanItem));
        modalApi.close();
        emit('success');
      } finally {
        modalApi.lock(false);
      }
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = modalApi.getData<MaintenancePlanApi.MaintenancePlanItem>();
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

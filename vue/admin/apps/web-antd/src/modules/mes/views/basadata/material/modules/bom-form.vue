<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useVbenModal } from '@vben/common-ui';
import { Button } from 'ant-design-vue';
import { useVbenForm } from '#/adapter/form';
import { $t } from '#/locales';

import type { BomApi } from '#/modules/mes/api/bom';
import { createBom, updateBom } from '#/modules/mes/api/bom';
import type { VbenFormSchema } from '#/adapter/form';

const props = defineProps<{ productId?: number }>();

const emit = defineEmits(['success']);
const formData = ref<BomApi.BomItem>();

const getTitle = computed(() => {
  return formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('bom.name')])
    : $t('ui.actionTitle.create', [$t('bom.name')]);
});

function useBomFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'code',
      label: $t('bom.code'),
      rules: 'required',
      formItemClass: 'col-span-1',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('bom.name'),
      rules: 'required',
      formItemClass: 'col-span-1',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full', disabled: true },
      fieldName: 'version',
      label: $t('bom.version'),
      formItemClass: 'col-span-1',
    },
    {
      component: 'DatePicker',
      fieldName: 'effDate',
      label: $t('bom.effDate'),
      formItemClass: 'col-span-1',
    },
    {
      component: 'DatePicker',
      fieldName: 'expDate',
      label: $t('bom.expDate'),
      formItemClass: 'col-span-1',
    },
    {
      component: 'InputTextArea',
      componentProps: { rows: 2 },
      fieldName: 'remark',
      label: $t('bom.remark'),
      formItemClass: 'col-span-2',
    },
  ];
}

const [Form, formApi] = useVbenForm({
  layout: 'vertical',
  schema: useBomFormSchema(),
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
      const payload: any = {
        ...data,
        productId: props.productId,
      };
      if (formData.value?.id) {
        payload.id = formData.value.id;
      }
      try {
        await (formData.value?.id
          ? updateBom(payload as BomApi.BomItem)
          : createBom(payload));
        modalApi.close();
        emit('success');
      } finally {
        modalApi.lock(false);
      }
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = modalApi.getData<BomApi.BomItem>();
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

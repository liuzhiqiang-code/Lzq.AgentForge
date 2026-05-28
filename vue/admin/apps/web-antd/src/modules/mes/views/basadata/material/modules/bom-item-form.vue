<script lang="ts" setup>
import { computed, ref } from 'vue';
import { useVbenModal } from '@vben/common-ui';
import { Button } from 'ant-design-vue';
import { useVbenForm } from '#/adapter/form';
import { $t } from '#/locales';

import type { BomApi } from '#/modules/mes/api/bom';
import { createBomItem, updateBomItem } from '#/modules/mes/api/bom';
import type { VbenFormSchema } from '#/adapter/form';

const props = defineProps<{ bomId?: number }>();

const emit = defineEmits(['success']);
const formData = ref<BomApi.BomItemDetail>();

const getTitle = computed(() => {
  return formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('bom.itemName')])
    : $t('ui.actionTitle.create', [$t('bom.itemName')]);
});

function useItemFormSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      fieldName: 'itemId',
      label: $t('bom.itemName'),
      rules: 'required',
      componentProps: {
        placeholder: $t('bom.itemName'),
        api: async () => {
          const { getMaterialSelectList: getList } = await import('#/modules/mes/api/material');
          const list = await getList();
          return (list || []).map((m: any) => ({ label: `[${m.code}] ${m.name}`, value: m.id }));
        },
      },
      formItemClass: 'col-span-2',
    },
    {
      component: 'InputNumber',
      fieldName: 'qty',
      label: $t('bom.qty'),
      rules: 'required',
      componentProps: { min: 0.0001, precision: 4 },
      formItemClass: 'col-span-1',
    },
    {
      component: 'InputNumber',
      fieldName: 'scrapRate',
      label: $t('bom.scrapRate'),
      componentProps: { min: 0, max: 100, precision: 2 },
      formItemClass: 'col-span-1',
    },
    {
      component: 'InputNumber',
      fieldName: 'sort',
      label: $t('bom.sort'),
      componentProps: { min: 0 },
      formItemClass: 'col-span-1',
    },
    {
      component: 'Input',
      fieldName: 'substituteIds',
      label: $t('bom.substituteIds'),
      componentProps: { placeholder: $t('bom.substituteIds') },
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
  schema: useItemFormSchema(),
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
        bomId: props.bomId,
      };
      if (formData.value?.id) {
        payload.id = formData.value.id;
      }
      try {
        await (formData.value?.id
          ? updateBomItem(payload)
          : createBomItem(payload));
        modalApi.close();
        emit('success');
      } finally {
        modalApi.lock(false);
      }
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = modalApi.getData<BomApi.BomItemDetail>();
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

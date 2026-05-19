<script lang="ts" setup>
import { computed, ref,nextTick } from 'vue';
import { useVbenModal } from '@vben/common-ui';
import { Button } from 'ant-design-vue';
import { useVbenForm } from '#/adapter/form';
import { $t } from '#/locales';

import ProviderSelector from './providerSelector.vue'

import type { ApiKeyApi } from '#/modules/ai/api/apiKey';
import { createApiKey, updateApiKey } from '#/modules/ai/api/apiKey';

import { useSchema } from '../data';


const emit = defineEmits(['success']);
const formData = ref<ApiKeyApi.ApiKey>();
const getTitle = computed(() => {
  return formData.value?.id
    ? $t('ui.actionTitle.edit', [$t('ai.apiKey.name')])
    : $t('ui.actionTitle.create', [$t('ai.apiKey.name')]);
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
          ? updateApiKey({ ...data, id: formData.value.id } as ApiKeyApi.ApiKey)
          : createApiKey(data as ApiKeyApi.ApiKey));
        modalApi.close();
        emit('success');
      } finally {
        modalApi.lock(false);
      }
    }
  },
  onOpenChange(isOpen) {
    if (isOpen) {
      const data = modalApi.getData<ApiKeyApi.ApiKey>();
      if (data) {
        formData.value = data;
        formApi.setValues(formData.value);

        // 编辑时处理模型选项和已选值
        if (data.id && data.showModel) {
          // 1. 先更新 selectModel 的选项列表
          formApi.setState((prev) => {
            const newSchema = prev.schema?.map((item) => {
              if (item.fieldName === 'selectModel') {
                return {
                  ...item,
                  componentProps: {
                    ...item.componentProps,
                    options: data.showModel.map((m: string) => ({ label: m, value: m })),
                  },
                };
              }
              return item;
            });
            return { schema: newSchema };
          });

          // 2. 再设置已选模型值
          nextTick(
            () => {
              formApi.setFieldValue('selectModel', data.selectModel || []);
            }
          );
        }
      }
    }
  },
});
</script>
<template>
  <Modal :title="getTitle">
    <Form class="mx-4">
      <!-- 厂商选择插槽 -->
      <template #provider="slotProps">
        <ProviderSelector :model-value="slotProps.modelValue" :disabled="!!formData?.id"
          @update:model-value="(val: any) => slotProps['onUpdate:modelValue'](val)" />
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

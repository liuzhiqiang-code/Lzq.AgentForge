<script lang="ts" setup>
import { ref } from 'vue';
import { useVbenModal } from '@vben/common-ui';
import { $t } from '#/locales';
import type { VbenFormProps } from '#/adapter/form';
import { useVbenVxeGrid } from '#/adapter/vxe-table';
import type { VxeTableGridOptions } from '#/adapter/vxe-table';

import type { AgentManageApi } from '#/modules/ai/api/agentManage';
import { getAgentManagePage } from '#/modules/ai/api/agentManage';

const emit = defineEmits(['success']);

const currentAgent = ref<AgentManageApi.AgentManage>()

const [Modal, modalApi] = useVbenModal({
  draggable: true,
  onOpenChange(isOpen: boolean) {
    if (isOpen) {
      const data = modalApi.getData<AgentManageApi.AgentManage>();
      currentAgent.value = data
    }
  },
  onOpened(){
    // if(currentAgent && currentAgent.value && currentAgent.value.name)
    // {
    //   gridApi.formApi.setFieldValue('name',currentAgent.value.name)
    //   gridApi.grid.setRadioRowKey(currentAgent.value.name)
    // }
  },
  onCancel() {
    modalApi.close();
  },
  onConfirm() {
    console.info('onConfirm');
    const data = gridApi.grid.getRadioRecord();
    console.info('data', data);
    emit('success', data);
    modalApi.close();
  },
  title: '请选择智能体',
});

const formOptions: VbenFormProps = {
  // 默认展开
  collapsed: false,
  schema: [
    {
      component: 'Input',
      componentProps: {
        placeholder: '',
        width: 200
      },
      defaultValue: '',
      fieldName: 'name',
      label: $t('ai.agentManage.name'),
    }
  ],

  // 控制表单是否显示折叠按钮
  showCollapseButton: false,
  submitButtonOptions: {
    content: '查询',
  },
  // 是否在字段值改变时提交表单
  submitOnChange: false,
  // 按下回车时是否提交表单
  submitOnEnter: false,
};

const gridOptions = {
  border: true,
  height: 300,
  rowConfig: {
    keyField: 'id',
    //isCurrent: true,
    //isHover: true
  },
  radioConfig: {
    highlight: true,
    // labelField: 'name',
    trigger: 'row'
  },
  // checkboxConfig: {
  //   highlight: true,
  //   labelField: 'name',
  // },
  columns: [
    {
      type: 'radio',
      width: 50,
      align: 'center',
    },
    {
      field: 'name',
      title: $t('ai.agentManage.name'), // 智能体名称
    },
    {
      field: 'description',
      title: $t('ai.agentManage.description'), // 智能体描述
    },
    {
      field: 'instructions',
      title: $t('ai.agentManage.instructions'), // 智能体提示词
    },
  ],
  exportConfig: {},
  // height: 'auto', // 如果设置为 auto，则必须确保存在父节点且不允许存在相邻元素，否则会出现高度闪动问题
  keepSource: true,
  proxyConfig: {
    ajax: {
      query: async ({ page }, formValues) => {
        if(currentAgent && currentAgent.value && currentAgent.value.name)
        {
          gridApi.formApi.setFieldValue('name',currentAgent.value.name)
          formValues.name = currentAgent.value.name
        }
        const data = await getAgentManagePage({
          page: page.currentPage,
          pageSize: page.pageSize,
          ...formValues,
        });
        setTimeout(() => {
          if (data && data.items && data.items[0]) {
            gridApi.grid.setRadioRow(data.items[0])
          }
        }, 200);
        return data
      },
    },
  },
  toolbarConfig: {
    custom: false,
    export: false,
    refresh: false,
    search: true,
    zoom: false,
  },
} as VxeTableGridOptions<AgentManageApi.AgentManage>

const [Grid, gridApi] = useVbenVxeGrid({
  formOptions,gridOptions,
});

</script>

<template>
  <Modal class="w-[60%] h-[50%] !h-[50%]" >
    <Grid/>
  </Modal>
</template>

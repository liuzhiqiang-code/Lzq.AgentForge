<script lang="ts" setup>
import { ref } from 'vue';
import { useVbenModal } from '@vben/common-ui';
import { Button, Card, Form, Input, message } from 'ant-design-vue';
import { executeAgentSkill } from '#/modules/ai/api/agentSkill';
import { $t } from '#/locales';

const executeLoading = ref(false);
const currentSkillName = ref('');
const currentTool = ref<any>(null);
const paramJson = ref('{}');
const resultContent = ref('');

const [Modal, modalApi] = useVbenModal({
  title: $t('ai.agentSkill.executeModalTitle'),
  footer: false,
  closeOnClickModal: false,
  draggable:true
});

function open(skillName: string, tool: any) {
  currentSkillName.value = skillName;
  currentTool.value = tool;
  const template: Record<string, string> = {};
  tool.parameters?.forEach((p: any) => {
    template[p.name] = '';
  });
  paramJson.value = JSON.stringify(template, null, 2);
  resultContent.value = '';
  modalApi.setState({ title: $t('ai.agentSkill.executeModalTitle') });
  modalApi.open();
}

async function handleExecute() {
  if (!currentSkillName.value || !currentTool.value) return;

  let args: Record<string, any> = {};
  try {
    args = JSON.parse(paramJson.value || '{}');
  } catch {
    message.error($t('ai.agentSkill.paramFormatError'));
    return;
  }

  executeLoading.value = true;
  try {
    const res = await executeAgentSkill({
      skillName: currentSkillName.value,
      toolName: currentTool.value.toolName,
      arguments: args,
    });
    resultContent.value = JSON.stringify(res, null, 2);
  } catch (error: any) {
    message.error(error?.message || $t('ai.agentSkill.executeFailed'));
  } finally {
    executeLoading.value = false;
  }
}

defineExpose({ open });
</script>

<template>
  <Modal>
    <div class="mb-4">
      <div class="font-medium">
        {{ $t('ai.agentSkill.currentTool', { toolName: currentTool?.toolName }) }}
      </div>
    </div>

    <Form layout="vertical">
      <Form.Item :label="$t('ai.agentSkill.paramJson')">
        <Input.TextArea
          v-model:value="paramJson"
          :rows="6"
          placeholder='例如: {"workOrder": "WO-20260427"}'
        />
      </Form.Item>

      <div v-if="resultContent" class="mt-4">
        <Card
          :title="$t('ai.agentSkill.executeResult')"
          size="small"
          :bordered="true"
          class="result-card"
        >
          <pre
            style="white-space: pre-wrap; word-break: break-word; margin: 0;"
            class="text-gray-900 dark:text-gray-200"
          >{{ resultContent }}</pre>
        </Card>
      </div>
    </Form>

    <div class="mt-4 flex justify-end">
      <Button @click="modalApi.close()">
        {{ $t('ai.agentSkill.cancel') }}
      </Button>
      <Button
        type="primary"
        :loading="executeLoading"
        class="ml-2"
        @click="handleExecute"
      >
        {{ $t('ai.agentSkill.execute') }}
      </Button>
    </div>
  </Modal>
</template>

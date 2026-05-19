<script lang="ts" setup>
import { ref } from 'vue';
import { Page } from '@vben/common-ui';
import { Button, message, Upload } from 'ant-design-vue';
import { MdiUpload } from '@vben/icons'
import {
  getAgentSkillList,
  uploadPlugin,
  uploadExternalZip,
  type AgentSkillApi
} from '#/modules/ai/api/agentSkill';
import { $t } from '#/locales';
import SkillCardList from './modules/skillCardList.vue';
import ExecuteSkillModal from './modules/executeSkillModal.vue';

const skills = ref<AgentSkillApi.SkillItem[]>([]);
const loading = ref(false);
const executeModalRef = ref<InstanceType<typeof ExecuteSkillModal>>();

async function loadSkills() {
  loading.value = true;
  try {
    skills.value = await getAgentSkillList();
  } catch {
    message.error($t('ai.agentSkill.loadError'));
  } finally {
    loading.value = false;
  }
}
loadSkills();

function handleExecute(skillName: string, tool: AgentSkillApi.ToolItem) {
  executeModalRef.value?.open(skillName, tool);
}

// ==================== 上传逻辑 ====================

/** DLL 上传 */
const pluginUploading = ref(false);
async function handleUploadPlugin(file: File) {
  if (!file.name.endsWith('.dll')) {
    message.error('只允许上传 .dll 文件');
    return false;
  }
  const hide = message.loading('正在上传程序集...', 0);
  try {
    await uploadPlugin(file);
    message.success('程序集上传成功');
    await loadSkills();   // 刷新技能列表
  } catch (e: any) {
    message.error(e?.message || '上传失败');
  } finally {
    hide();
  }
  return false; // 阻止默认上传行为
}

/** ZIP 上传 */
const zipUploading = ref(false);
async function handleUploadExternal(file: File) {
  if (!file.name.endsWith('.zip')) {
    message.error('只允许上传 .zip 文件');
    return false;
  }
  const hide = message.loading('正在解压外部技能...', 0);
  try {
    await uploadExternalZip(file);
    message.success('外部技能包上传成功');
    await loadSkills();
  } catch (e: any) {
    message.error(e?.message || '上传失败');
  } finally {
    hide();
  }
  return false;
}
</script>

<template>
  <Page auto-content-height>
    <div class="p-4">
      <div class="mb-4 flex items-center justify-between">
        <h2 class="text-xl font-bold">
          {{ $t('ai.agentSkill.title') }}
        </h2>
        <div class="flex gap-2">
          <!-- 上传程序集 -->
          <Upload :show-upload-list="false" :before-upload="handleUploadPlugin" accept=".dll">
            <Button :loading="pluginUploading">
              <MdiUpload class="mr-1" /> 上传程序集
            </Button>
          </Upload>
          <!-- 上传外部技能包 -->
          <Upload :show-upload-list="false" :before-upload="handleUploadExternal" accept=".zip">
            <Button :loading="zipUploading">
              <MdiUpload class="mr-1" /> 上传外部技能
            </Button>
          </Upload>
        </div>

      </div>


      <SkillCardList :skills="skills" @execute="handleExecute" />
      <ExecuteSkillModal ref="executeModalRef" />
    </div>
  </Page>
</template>

import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'ion:settings-outline',
      order: 9997,
      title: $t('ai.title'),
    },
    name: 'ai',
    path: '/ai',
    children: [
      {
        path: '/ai/chats',
        name: 'chats',
        meta: {
          icon: 'mdi:robot-happy-outline',          // 聊天
          title: $t('ai.chats.title'),
        },
        component: () => import('#/modules/ai/views/chats/index.vue'),
      },
      {
        path: '/ai/agentManage',
        name: 'agentManage',
        meta: {
          icon: 'mdi:robot-outline',         // 代理管理
          title: $t('ai.agentManage.title'),
        },
        component: () => import('#/modules/ai/views/agentManage/list.vue'),
      },
      {
        path: '/ai/agentSkill',
        name: 'agentSkill',
        meta: {
          icon: 'mdi:creation-outline',      // 修正为与后端一致的“技能/创意”图标
          title: $t('ai.agentSkill.title'),
        },
        component: () => import('#/modules/ai/views/agentSkill/index.vue'),
      },
      {
        path: '/ai/modelConfig',
        name: 'modelConfig',
        meta: {
          icon: 'mdi:brain',         // 修正为与后端一致的“模型/大脑”图标
          title: $t('ai.modelConfig.title'),
        },
        component: () => import('#/modules/ai/views/modelConfig/list.vue'),
      },
      {
        path: '/ai/apiKey',
        name: 'apiKey',
        meta: {
          icon: 'mdi:key-outline',           // API 密钥
          title: $t('ai.apiKey.title'),
        },
        component: () => import('#/modules/ai/views/apiKey/list.vue'),
      },
    ],
  },
];

export default routes;

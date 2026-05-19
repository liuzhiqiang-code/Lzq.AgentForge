import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'ion:server-outline',
      order: 1000,
      title: $t('basadata.title'),
    },
    name: 'BaseData',
    path: '/basadata',
    children: [
      {
        path: '/basadata/factory',
        name: 'Factory',
        meta: {
          icon: 'mdi:factory',
          title: $t('basadata.factory.title'),
        },
        component: () => import('#/modules/basadata/views/factory/list.vue'),
      },
      {
        path: '/basadata/workshop',
        name: 'Workshop',
        meta: {
          icon: 'mdi:warehouse',
          title: $t('basadata.workshop.title'),
        },
        component: () => import('#/modules/basadata/views/workshop/list.vue'),
      },
      {
        path: '/basadata/line',
        name: 'Line',
        meta: {
          icon: 'mdi:sort-variant',
          title: $t('basadata.line.title'),
        },
        component: () => import('#/modules/basadata/views/line/list.vue'),
      },
      {
        path: '/basadata/process',
        name: 'Process',
        meta: {
          icon: 'mdi:cog-sync-outline',
          title: $t('basadata.process.title'),
        },
        component: () => import('#/modules/basadata/views/process/list.vue'),
      },
    ],
  },
];

export default routes;

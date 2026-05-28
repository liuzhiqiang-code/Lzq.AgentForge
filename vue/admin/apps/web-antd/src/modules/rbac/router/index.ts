import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'ion:settings-outline',
      order: -1,
      title: $t('rbac.title'),
    },
    name: 'Rbac',
    path: '/system',
    children: [
      {
        path: '/system/role',
        name: 'RbacRole',
        meta: {
          icon: 'mdi:account-group',
          title: $t('rbac.role.title'),
        },
        component: () => import('#/modules/rbac/views/role/list.vue'),
      },
      {
        path: '/system/menu',
        name: 'RbacMenu',
        meta: {
          icon: 'mdi:menu',
          title: $t('rbac.menu.title'),
        },
        component: () => import('#/modules/rbac/views/menu/list.vue'),
      },
      {
        path: '/system/dept',
        name: 'RbacDept',
        meta: {
          icon: 'charm:organisation',
          title: $t('rbac.dept.title'),
        },
        component: () => import('#/modules/rbac/views/dept/list.vue'),
      },
    ],
  },
];

export default routes;

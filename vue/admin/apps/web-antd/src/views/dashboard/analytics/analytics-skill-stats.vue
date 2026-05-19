<script lang="ts" setup>
import type { EchartsUIType } from '@vben/plugins/echarts';
import { onMounted, ref } from 'vue';
import { EchartsUI, useEcharts } from '@vben/plugins/echarts';
import { getSkillStats } from '#/api/dashboard/analytics';

const chartRef = ref<EchartsUIType>();
const { renderEcharts } = useEcharts(chartRef);

async function initChart() {
  const data = await getSkillStats();

  renderEcharts({
    tooltip: { trigger: 'item' },
    series: [
      {
        name: '技能统计',
        type: 'pie',
        radius: ['15%', '80%'], // 增加内径，做成环形玫瑰图
        center: ['50%', '50%'],
        roseType: 'area',
        color: ['#2ec7c9', '#b6a2de', '#5ab1ef', '#ffb980', '#d87a80'],
        data: data.map(item => ({ name: item.name, value: item.count }))
                 .toSorted((a, b) => a.value - b.value),
        itemStyle: {
          borderRadius: 6
        },
        animationType: 'scale',
        animationEasing: 'cubicOut'
      },
    ],
  });
}

onMounted(initChart);
</script>

<template>
  <EchartsUI ref="chartRef" />
</template>

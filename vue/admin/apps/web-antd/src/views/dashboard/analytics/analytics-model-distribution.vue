<script lang="ts" setup>
import type { EchartsUIType } from '@vben/plugins/echarts';
import { onMounted, ref } from 'vue';
import { EchartsUI, useEcharts } from '@vben/plugins/echarts';
import { getModelDistribution } from '#/api/dashboard/analytics';

const chartRef = ref<EchartsUIType>();
const { renderEcharts } = useEcharts(chartRef);

async function initChart() {
  const data = await getModelDistribution();

  renderEcharts({
    tooltip: { trigger: 'item' },
    series: [
      {
        name: '模型分布',
        type: 'pie',
        radius: '80%',
        center: ['50%', '50%'],
        roseType: 'radius',
        color: ['#5ab1ef', '#b6a2de', '#67e0e3', '#2ec7c9', '#ffb980'],
        data: data.toSorted((a, b) => a.value - b.value),
        animationType: 'scale',
        animationEasing: 'exponentialInOut',
        animationDelay() {
          return Math.random() * 400;
        },
      },
    ],
  });
}

onMounted(initChart);
</script>

<template>
  <EchartsUI ref="chartRef" />
</template>

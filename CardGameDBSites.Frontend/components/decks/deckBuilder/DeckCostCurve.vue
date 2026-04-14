<script setup lang="ts">
import { Bar } from "vue-chartjs";
import {
  Chart as ChartJS,
  Title,
  Tooltip,
  Legend,
  BarElement,
  CategoryScale,
  LinearScale,
} from "chart.js";
import type { CreateDeckModel } from "./models/CreateDeckModel";
import { computeCostCurveChart } from "~/helpers/CostCurveHelper";

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale);

const TYPE_COLORS = [
  "#4B6BFB",
  "#F5A623",
  "#E74C3C",
  "#2ECC71",
  "#9B59B6",
  "#1ABC9C",
  "#E67E22",
  "#3498DB",
  "#F39C12",
  "#D35400",
];

const props = defineProps<{
  deck: CreateDeckModel;
  costAttribute?: string;
  typeAttribute?: string;
}>();

const chartData = computed(() => {
  const raw = computeCostCurveChart(
    props.deck,
    props.costAttribute ?? "Cost",
    props.typeAttribute,
  );
  return {
    labels: raw.labels,
    datasets: raw.datasets.map((ds, i) => ({
      label: ds.label,
      data: ds.data,
      backgroundColor: TYPE_COLORS[i % TYPE_COLORS.length],
    })),
  };
});

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    title: {
      display: true,
      text: "Cost curve",
      color: "#ffffff",
      font: { size: 14, weight: "bold" as const },
    },
    legend: {
      display: true,
      labels: { color: "#ffffff" },
    },
  },
  scales: {
    x: {
      stacked: true,
      grid: { color: "rgba(255,255,255,0.15)" },
      ticks: { color: "#ffffff" },
    },
    y: {
      stacked: true,
      beginAtZero: true,
      grid: { color: "rgba(255,255,255,0.15)" },
      ticks: { color: "#ffffff", stepSize: 1 },
    },
  },
};

const hasData = computed(() =>
  chartData.value.datasets.some((ds) => ds.data.some((v) => v > 0)),
);
</script>

<template>
  <div v-if="hasData" class="mt-4 rounded p-2" style="background-color: #000; height: 240px;">
    <ClientOnly>
      <Bar :data="chartData" :options="chartOptions" />
    </ClientOnly>
  </div>
</template>

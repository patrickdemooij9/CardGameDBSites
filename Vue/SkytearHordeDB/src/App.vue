<script lang="ts">
export default {
  setup(props, ctx) {
    return () => ctx.slots.default!()
  }
}
</script>
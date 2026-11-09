<script setup lang="ts">
useHead({ title: 'Search posts' })

const route = useRoute()
const router = useRouter()

const query = ref(typeof route.query.q === 'string' ? route.query.q : '')
const submitted = computed(() => (typeof route.query.q === 'string' ? route.query.q : ''))
const isLoading = ref(false)
const errorMsg = ref<string | null>(null)
const results = ref<Array<{ userId: number, posts: Array<{ id: number, title: string, body: string }> }>>([])
const lastSearchQuery = ref('')

onMounted(() => {
  if (submitted.value && !isLoading.value) {
    onSearch()
  }
})

async function onSearch() {
  const q = query.value.trim()

  if (isLoading.value) return
  if (q === lastSearchQuery.value) return

  if (!q) {
    results.value = []
    lastSearchQuery.value = ''
    return
  }

  isLoading.value = true
  lastSearchQuery.value = q
  errorMsg.value = null

  router.replace({ query: { q } })

  try {
    results.value = await $fetch(
      `http://localhost:5286/post/search?q=${encodeURIComponent(q)}`,
      { method: 'GET' }
    )
  } catch (error) {
    errorMsg.value = 'An error occurred while searching.'
    results.value = []
    lastSearchQuery.value = ''
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="min-h-dvh flex flex-col">
    <header class="sticky top-0 z-10 border-b border-lichen bg-fog/95 backdrop-blur">
      <div class="mx-auto flex max-w-5xl flex-col gap-3 px-4 py-4 sm:flex-row sm:items-center sm:gap-8 sm:px-6">
        <NuxtLink
          to="/search"
          class="font-display text-2xl font-extrabold tracking-tight text-ink"
        >
          Posts
        </NuxtLink>

        <form
          role="search"
          class="flex flex-1 items-stretch"
          @submit.prevent="onSearch"
        >
          <label for="search-input" class="sr-only">Search posts</label>
          <input
            id="search-input"
            v-model="query"
            type="search"
            name="q"
            autocomplete="off"
            placeholder="Search posts by title"
            class="h-12 min-w-0 flex-1 rounded-l-lg border-2 border-r-0 border-ink bg-field px-4 text-base text-ink placeholder:text-stone focus-visible:outline-offset-0"
          >
          <button
            type="submit"
            class="h-12 shrink-0 rounded-r-lg border-2 border-ink bg-moss px-5 font-semibold text-field transition-colors hover:bg-moss-deep active:bg-ink"
          >
            Search
            
          </button>
        </form>
      </div>
    </header>

    <main class="mx-auto w-full max-w-5xl flex-1 px-4 py-8 sm:px-6">
      <section
        aria-live="polite"
        class="flex min-h-80 flex-col justify-center rounded-lg border-2 border-dashed border-lichen px-6 py-10"
      >
       <p class="font-display text-xl font-semibold">
            Find a post
          </p>
          <p class="mt-2 max-w-prose text-stone">
            Type part of a title in the search box, then press Enter or select Search.
          </p>
      </section>
      <section
        class="mt-8 flex flex-col gap-4"
      >
        <div v-if="results.length === 0 && !isLoading && !errorMsg">
          No results found.
        </div>
        <div v-if="isLoading">
          <p class="text-stone">Loading...</p>
        </div>
        <div v-if="errorMsg">
          <p class="text-red-600">{{ errorMsg }}</p>
        </div>
        <div v-if="results.length > 0">
          <p class="text-stone">Total results: {{ results.length }}</p>
          <div class="rounded-lg border-2 border-lichen p-4" v-for="group in results" :key="group.userId">
            <h2 class="font-display text-lg font-semibold">User {{ group.userId }}</h2>
            <div class="flex flex-col gap-4">
              <div v-for="post in group.posts" :key="post.id" class="rounded-lg border-2 border-lichen p-4">
                <h3 class="font-display text-md font-semibold">{{ post.title }}</h3>
                <p class="text-stone">{{ post.body }}</p>
              </div>
            </div>
          </div>
        </div>
        
      </section>
    </main>
  </div>
</template>

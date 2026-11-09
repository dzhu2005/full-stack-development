<script setup lang="ts">
useHead({ title: 'Search posts' })

interface Post {
  userId: number
  id: number
  title: string
  body: string
}

interface UserPostGroup {
  userId: number
  posts: Post[]
}

const route = useRoute()
const router = useRouter()
const config = useRuntimeConfig()

const query = ref(typeof route.query.q === 'string' ? route.query.q : '')
const submitted = computed(() =>
  typeof route.query.q === 'string' ? route.query.q : ''
)

const results = ref<UserPostGroup[]>([])
const loading = ref(false)
const error = ref('')

async function searchPosts() {
  if (!submitted.value) {
    results.value = []
    error.value = ''
    return
  }

  loading.value = true
  error.value = ''

  try {
    results.value = await $fetch<UserPostGroup[]>(
      `${config.public.apiBase}/Post/search`,
      { query: { q: submitted.value } }
    )
  } catch {
    results.value = []
    error.value = 'Unable to load posts. Please try again.'
  } finally {
    loading.value = false
  }
}

function onSearch() {
  const q = query.value.trim()
  router.replace({ query: q ? { q } : {} })
}

watch(submitted, searchPosts, { immediate: true })
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
        <p v-if="loading" class="font-display text-xl font-semibold">
          Searching posts...
        </p>

        <p v-else-if="error" class="font-display text-xl font-semibold text-red-700">
          {{ error }}
        </p>

        <template v-else-if="!submitted">
          <p class="font-display text-xl font-semibold">Find a post</p>
          <p class="mt-2 max-w-prose text-stone">
            Type part of a title in the search box, then press Enter or select Search.
          </p>
        </template>

        <p v-else-if="results.length === 0" class="font-display text-xl font-semibold">
          No posts found.
        </p>

        <div v-else class="space-y-6">
          <section
            v-for="group in results"
            :key="group.userId"
            class="rounded-lg border border-lichen bg-field p-5"
          >
            <h2 class="mb-4 font-display text-lg font-bold">
              User {{ group.userId }}
            </h2>

            <div
              v-for="post in group.posts"
              :key="post.id"
              class="border-t border-lichen py-4"
            >
              <h3 class="font-semibold">{{ post.title }}</h3>
              <p class="mt-2 text-sm text-stone">{{ post.body }}</p>
            </div>
          </section>
        </div>
      </section>
    </main>
  </div>
</template>

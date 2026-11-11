<script setup lang="ts">
useHead({ title: 'Search posts' })

const route = useRoute()
const router = useRouter()

const query = ref(typeof route.query.q === 'string' ? route.query.q : '')
const submitted = computed(() => (typeof route.query.q === 'string' ? route.query.q : ''))

const { results, isLoading, errorMessage, search, reset } = usePostSearch()

/** Total number of matching posts across all users. */
const postCount = computed(() =>
  (results.value ?? []).reduce((total, group) => total + group.posts.length, 0)
)

/**
 * Puts the trimmed search term in the URL (`?q=`), which triggers the search below.
 * Re-submitting the same term runs the search again, because the URL does not change.
 */
function onSearch() {
  const q = query.value.trim()
  if (q && q === submitted.value) {
    search(q)
    return
  }
  router.replace({ query: q ? { q } : {} })
}

/**
 * Runs the search for a term, or clears the results when the term is empty.
 * @param term The submitted search term from the URL.
 */
function runSearch(term: string) {
  if (term) {
    search(term)
  }
  else {
    reset()
  }
}

/** Re-runs the last search, used by the "Try again" button. */
function retry() {
  search(submitted.value)
}

// The URL is the source of truth, so back/forward and shared links work.
watch(submitted, (term) => {
  query.value = term
  runSearch(term)
})

// Fetch on the client only: the first search runs once the page is mounted.
onMounted(() => runSearch(submitted.value))
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
            :disabled="isLoading"
            class="h-12 shrink-0 rounded-r-lg border-2 border-ink bg-moss px-5 font-semibold text-field transition-colors hover:bg-moss-deep active:bg-ink disabled:cursor-wait disabled:opacity-70"
          >
            Search
          </button>
        </form>
      </div>
    </header>

    <main class="mx-auto w-full max-w-5xl flex-1 px-4 py-8 sm:px-6">
      <section aria-live="polite" :aria-busy="isLoading">
        <!-- Loading -->
        <div
          v-if="isLoading"
          class="flex min-h-80 flex-col items-center justify-center gap-3 rounded-lg border-2 border-dashed border-lichen px-6 py-10"
        >
          <span class="size-8 animate-spin rounded-full border-4 border-lichen border-t-moss" aria-hidden="true" />
          <p class="text-stone">
            Searching for “{{ submitted }}”…
          </p>
        </div>

        <!-- Error -->
        <div
          v-else-if="errorMessage"
          role="alert"
          class="flex min-h-80 flex-col justify-center rounded-lg border-2 border-dashed border-red-300 px-6 py-10"
        >
          <p class="font-display text-xl font-semibold">
            Something went wrong
          </p>
          <p class="mt-2 max-w-prose text-stone">
            {{ errorMessage }}
          </p>
          <button
            type="button"
            class="mt-4 self-start rounded-lg border-2 border-ink bg-field px-4 py-2 font-semibold text-ink transition-colors hover:bg-lichen"
            @click="retry"
          >
            Try again
          </button>
        </div>

        <!-- No results -->
        <div
          v-else-if="results && results.length === 0"
          class="flex min-h-80 flex-col justify-center rounded-lg border-2 border-dashed border-lichen px-6 py-10"
        >
          <p class="font-display text-xl font-semibold">
            No posts match “{{ submitted }}”
          </p>
          <p class="mt-2 max-w-prose text-stone">
            Try a shorter or different word from the title.
          </p>
        </div>

        <!-- Results grouped by user -->
        <div v-else-if="results">
          <p class="text-stone">
            {{ postCount }} {{ postCount === 1 ? 'post' : 'posts' }} from
            {{ results.length }} {{ results.length === 1 ? 'user' : 'users' }} match “{{ submitted }}”
          </p>

          <section
            v-for="group in results"
            :key="group.userId"
            :aria-labelledby="`user-${group.userId}`"
            class="mt-8"
          >
            <h2 :id="`user-${group.userId}`" class="font-display text-xl font-semibold">
              User {{ group.userId }}
              <span class="ml-1 text-base font-normal text-stone">
                · {{ group.posts.length }} {{ group.posts.length === 1 ? 'post' : 'posts' }}
              </span>
            </h2>

            <ul class="mt-3 grid gap-3 sm:grid-cols-2">
              <li
                v-for="post in group.posts"
                :key="post.id"
                class="rounded-lg border-2 border-lichen bg-field p-4"
              >
                <h3 class="font-semibold first-letter:uppercase">
                  {{ post.title }}
                </h3>
                <p class="mt-2 line-clamp-3 whitespace-pre-line text-sm text-stone">
                  {{ post.body }}
                </p>
              </li>
            </ul>
          </section>
        </div>

        <!-- Idle -->
        <div
          v-else
          class="flex min-h-80 flex-col justify-center rounded-lg border-2 border-dashed border-lichen px-6 py-10"
        >
          <p class="font-display text-xl font-semibold">
            Find a post
          </p>
          <p class="mt-2 max-w-prose text-stone">
            Type part of a title in the search box, then press Enter or select Search.
          </p>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup lang="ts">
useHead({ title: 'Search posts' })

const route = useRoute()
const router = useRouter()

// The URL is the single source of truth for what has been searched.
const query = ref(typeof route.query.q === 'string' ? route.query.q : '')
const submitted = computed(() => (typeof route.query.q === 'string' ? route.query.q.trim() : ''))

// Deliberately not awaited: the request must never block the page from rendering,
// so the loading state below can be shown while the API is working.
const { data: groups, status, error, refresh } = usePostSearch(submitted)

const totalPosts = computed(() => groups.value.reduce((total, group) => total + group.posts.length, 0))
const totalUsers = computed(() => groups.value.length)

const errorMessage = computed(() => {
  const failure = error.value as { statusCode?: number, status?: number } | undefined
  const statusCode = failure?.statusCode ?? failure?.status

  if (statusCode === 400) {
    return 'That search term was not accepted. Try a different word.'
  }

  // 502/504 come from the API when the upstream posts service fails; no status code at all
  // means the API itself could not be reached.
  if (!statusCode || statusCode === 502 || statusCode === 504) {
    return 'The post service is unavailable right now. Please try again in a moment.'
  }

  return 'Something went wrong while searching posts. Please try again.'
})

// Keep the input in sync when the URL changes (back/forward navigation, shared links).
watch(() => route.query.q, (value) => {
  query.value = typeof value === 'string' ? value : ''
})

function onSearch() {
  const q = query.value.trim()

  if (q === submitted.value) {
    // Same term: re-run the request so a failed search can be retried.
    if (q) {
      refresh()
    }

    return
  }

  router.replace({ query: q ? { q } : {} })
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
      <!-- Idle: nothing searched yet -->
      <section
        v-if="!submitted"
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

      <!-- Loading -->
      <section
        v-else-if="status === 'pending'"
        role="status"
        aria-live="polite"
        aria-busy="true"
        class="flex min-h-80 flex-col justify-center rounded-lg border-2 border-dashed border-lichen px-6 py-10"
      >
        <p class="font-display text-xl font-semibold">
          Searching…
        </p>
        <p class="mt-2 max-w-prose text-stone">
          Looking for posts matching “{{ submitted }}”.
        </p>
        <div class="mt-6 space-y-3" aria-hidden="true">
          <div class="h-4 w-2/3 animate-pulse rounded bg-lichen" />
          <div class="h-4 w-1/2 animate-pulse rounded bg-lichen" />
          <div class="h-4 w-3/5 animate-pulse rounded bg-lichen" />
        </div>
      </section>

      <!-- Error -->
      <section
        v-else-if="error"
        role="alert"
        class="flex min-h-80 flex-col justify-center rounded-lg border-2 border-ink/20 bg-field px-6 py-10"
      >
        <p class="font-display text-xl font-semibold">
          We couldn’t load posts
        </p>
        <p class="mt-2 max-w-prose text-stone">
          {{ errorMessage }}
        </p>
        <div>
          <button
            type="button"
            class="mt-6 h-11 rounded-lg border-2 border-ink bg-moss px-5 font-semibold text-field transition-colors hover:bg-moss-deep active:bg-ink"
            @click="refresh()"
          >
            Try again
          </button>
        </div>
      </section>

      <!-- No results -->
      <section
        v-else-if="totalPosts === 0"
        aria-live="polite"
        class="flex min-h-80 flex-col justify-center rounded-lg border-2 border-dashed border-lichen px-6 py-10"
      >
        <p class="font-display text-xl font-semibold">
          No posts found
        </p>
        <p class="mt-2 max-w-prose text-stone">
          Nothing matched “{{ submitted }}”. Try a shorter or different word.
        </p>
      </section>

      <!-- Results, grouped by user -->
      <div v-else>
        <p class="mb-6 text-sm text-stone" aria-live="polite">
          <span class="font-semibold text-ink">{{ totalPosts }}</span>
          {{ totalPosts === 1 ? 'post' : 'posts' }}
          across
          <span class="font-semibold text-ink">{{ totalUsers }}</span>
          {{ totalUsers === 1 ? 'user' : 'users' }}
          for “{{ submitted }}”
        </p>

        <div class="space-y-6">
          <section
            v-for="group in groups"
            :key="group.userId"
            class="overflow-hidden rounded-lg border-2 border-lichen bg-field"
          >
            <header class="flex items-baseline justify-between gap-4 border-b-2 border-lichen px-5 py-3">
              <h2 class="font-display text-lg font-semibold">
                User {{ group.userId }}
              </h2>
              <span class="shrink-0 text-sm text-stone">
                {{ group.posts.length }} {{ group.posts.length === 1 ? 'post' : 'posts' }}
              </span>
            </header>

            <ul class="divide-y divide-lichen">
              <li
                v-for="post in group.posts"
                :key="post.id"
                class="px-5 py-4"
              >
                <article>
                  <h3 class="font-semibold text-pretty">
                    {{ post.title }}
                  </h3>
                  <p class="mt-1 line-clamp-2 text-sm text-stone">
                    {{ post.body }}
                  </p>
                </article>
              </li>
            </ul>
          </section>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
useHead({ title: 'Search posts' })

const query = ref('')
const submitted = ref('')
const isLoading = ref(false)
const errorMsg = ref<string | null>(null)
const results = ref<any[] | null>(null)

async function onSearch() {
  const q = query.value.trim()
  submitted.value = q

  if (!q) {
    results.value = null
    return
  }

  isLoading.value = true
  errorMsg.value = null

  try {
    results.value = await $fetch(`http://localhost:5286/Post/search?q=${encodeURIComponent(q)}`)
  } catch (err) {
    errorMsg.value = 'Failed to load results. Please ensure the backend is running on port 5286.'
    results.value = null
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="min-h-dvh flex flex-col">
    <header class="sticky top-0 z-10 border-b border-lichen bg-fog/95 backdrop-blur">
      <div class="mx-auto flex max-w-5xl flex-col gap-3 px-4 py-4 sm:flex-row sm:items-center sm:gap-8 sm:px-6">
        <NuxtLink to="/search" class="font-display text-2xl font-extrabold tracking-tight text-ink">
          Posts
        </NuxtLink>

        <form role="search" class="flex flex-1 items-stretch" @submit.prevent="onSearch">
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
            class="h-12 shrink-0 rounded-r-lg border-2 border-ink bg-moss px-5 font-semibold text-field transition-colors hover:bg-moss-deep active:bg-ink disabled:opacity-70"
          >
            {{ isLoading ? 'Searching...' : 'Search' }}
          </button>
        </form>
      </div>
    </header>

    <main class="mx-auto w-full max-w-5xl flex-1 px-4 py-8 sm:px-6">
      <ClientOnly>
        <!-- Initial State -->
        <section v-if="!submitted" class="flex min-h-80 flex-col justify-center rounded-lg border-2 border-dashed border-lichen px-6 py-10">
          <p class="font-display text-xl font-semibold">Find a post</p>
          <p class="mt-2 max-w-prose text-stone">Type part of a title in the search box, then press Enter or select Search.</p>
        </section>

        <!-- Loading State -->
        <section v-else-if="isLoading" class="flex min-h-80 flex-col justify-center items-center rounded-lg border-2 border-dashed border-lichen px-6 py-10">
          <p class="font-display text-xl font-semibold text-stone animate-pulse">Searching...</p>
        </section>

        <!-- Error State -->
        <section v-else-if="errorMsg" class="flex min-h-80 flex-col justify-center rounded-lg border-2 border-red-300 bg-red-50 px-6 py-10">
          <p class="font-display text-xl font-semibold text-red-700">Connection Error</p>
          <p class="mt-2 max-w-prose text-red-600">{{ errorMsg }}</p>
        </section>

        <!-- Empty State -->
        <section v-else-if="results && results.length === 0" class="flex min-h-80 flex-col justify-center rounded-lg border-2 border-dashed border-lichen px-6 py-10">
          <p class="font-display text-xl font-semibold">No results found</p>
          <p class="mt-2 max-w-prose text-stone">We couldn't find any posts matching "<span class="font-semibold text-ink">{{ submitted }}</span>".</p>
        </section>

        <!-- Results State -->
        <section v-else-if="results && results.length > 0" class="flex flex-col gap-6">
          <div v-for="group in results" :key="group.userId" class="rounded-lg border-2 border-lichen bg-fog/30 p-6">
            <h2 class="mb-4 font-display text-xl font-bold text-ink">User {{ group.userId }}</h2>
            <div class="flex flex-col gap-4">
              <article v-for="post in group.posts" :key="post.id" class="rounded border border-lichen bg-field p-4 shadow-sm">
                <h3 class="mb-2 font-display text-lg font-semibold capitalize text-ink">{{ post.title }}</h3>
                <p class="text-stone">{{ post.body }}</p>
              </article>
            </div>
          </div>
        </section>
      </ClientOnly>
    </main>
  </div>
</template>

import type { UserPosts } from '~/types/post'

/**
 * Calls the post search API and keeps the state the search page needs:
 * the grouped results, a loading flag and an error message.
 */
export function usePostSearch() {
  const config = useRuntimeConfig()

  const results = ref<UserPosts[] | null>(null)
  const isLoading = ref(false)
  const errorMessage = ref<string | null>(null)

  /**
   * Searches posts whose title contains `term` and stores the grouped results.
   * On failure, `errorMessage` is set and `results` is cleared.
   * @param term Text to look for in post titles. Must not be empty.
   */
  async function search(term: string) {
    isLoading.value = true
    errorMessage.value = null

    try {
      results.value = await $fetch<UserPosts[]>('/Post/search', {
        baseURL: config.public.apiBaseUrl,
        query: { q: term }
      })
    }
    catch {
      results.value = null
      errorMessage.value = 'We could not load posts right now. Please try again.'
    }
    finally {
      isLoading.value = false
    }
  }

  /** Clears results and errors, e.g. when the search box is emptied. */
  function reset() {
    results.value = null
    errorMessage.value = null
    isLoading.value = false
  }

  return { results, isLoading, errorMessage, search, reset }
}

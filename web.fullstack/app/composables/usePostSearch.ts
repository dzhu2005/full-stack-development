import { toValue, type MaybeRefOrGetter } from 'vue'
import type { UserPostsGroup } from '~/types/post'

/**
 * Searches posts on the back end and re-runs whenever the search term changes.
 *
 * A blank term is never sent to the API (the endpoint answers 400 for blank terms),
 * so the page simply resolves to "no results" until the user types something.
 */
export function usePostSearch(term: MaybeRefOrGetter<string>) {
  const apiBase = useRuntimeConfig().public.apiBase

  return useAsyncData<UserPostsGroup[]>(
    'post-search',
    async () => {
      const q = toValue(term).trim()

      if (!q) {
        return []
      }

      return await $fetch<UserPostsGroup[]>('/Post/search', {
        baseURL: apiBase,
        query: { q }
      })
    },
    {
      default: () => [],
      watch: [() => toValue(term)],
      // Do not block rendering while the request is in flight: the page keeps the search box
      // on screen and shows its own loading state.
      lazy: true
    }
  )
}

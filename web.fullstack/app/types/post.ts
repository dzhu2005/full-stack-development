/**
 * A post as returned by the back-end search endpoint.
 */
export interface Post {
  userId: number
  id: number
  title: string
  body: string
}

/**
 * The posts that matched a search term, grouped under the user who wrote them.
 * Mirrors `UserPostsGroup` on the API.
 */
export interface UserPostsGroup {
  userId: number
  posts: Post[]
}

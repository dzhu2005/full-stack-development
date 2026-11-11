/** A single post, as returned by the API. */
export interface Post {
  userId: number
  id: number
  title: string
  body: string
}

/** Matching posts grouped under the user who wrote them (one item of `GET /Post/search`). */
export interface UserPosts {
  userId: number
  posts: Post[]
}

namespace api.fullstack.Dtos
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }

    public class UserPostsDto
    {
        public int UserId { get; set; }
        public List<PostDto> Posts { get; set; }
    }
}

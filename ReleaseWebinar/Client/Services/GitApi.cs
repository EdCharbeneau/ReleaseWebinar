using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ReleaseWebinar.Client
{
    public class GitHubApi
    {
        public GitHubApi(HttpClient http)
        {
            Http = http;
        }

        public HttpClient Http { get; }

        public async Task<List<TreeNode>> GetProjectRootNodes(string user, string project)
        {
            var result = await Http.GetFromJsonAsync<TreeNode>($"https://api.github.com/repos/{user}/{project}/git/trees/master");
            return result.Tree.FilterMarkdownNodes().ToList();
        }

        public async Task<IEnumerable<TreeNode>> GetProjectNodes(string uri)
        {
            var result = await Http.GetFromJsonAsync<TreeNode>(uri);
            return result.Tree.FilterMarkdownNodes().AddParentSha(result.Sha);
        }

        public async Task<string> GetFileResponseAsync(string uri)
        {
            var result = await Http.GetFromJsonAsync<GitFileResponse>(uri);
            
            return Encoding.UTF8.GetString(Convert.FromBase64String(result.Content));
        }

    }
    public static class GitHubApiFilters
    {
        public static IEnumerable<TreeNode> FilterMarkdownNodes(this IList<TreeNode> nodes) => nodes.Where(t => t.Path.Contains(".md") || t.Type == "tree");
        public static List<TreeNode> AddParentSha(this IEnumerable<TreeNode> nodes, string parentSha) => nodes.Select(x => { x.ParentSha = parentSha; return x; }).ToList();
    }

    public class TreeNode
    {
        public string Path { get; set; } = "root";
        public int Size { get; set; }
        public string Sha { get; set; }
        public string ParentSha { get; set; }
        public TreeNode[] Tree { get; set; } = Array.Empty<TreeNode>();
        public string Type { get; set; }
        public bool HasChildren => Type == "tree";
        public string Url { get; set; }

    }

    public class GitFileResponse
    {
        public string Content { get; set; }
    }
}

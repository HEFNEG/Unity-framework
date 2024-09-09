using System.Collections.Generic;
using System.IO;
using LitJson;

namespace Game.Basic {
    public class LetterTreeNode {
        private Dictionary<char, LetterTreeNode> m_nodeDict = new Dictionary<char, LetterTreeNode>();

        public readonly string Str;

        public LetterTreeNode(string str = "") {
            Str = str;
        }

        public bool TryGetNode(char c, out LetterTreeNode node) {
            return m_nodeDict.TryGetValue(c, out node);
        }

        public void InsertNode(char c, LetterTreeNode nextNode) {
            m_nodeDict.TryAdd(c, nextNode);
        }
        
        public bool IsChildEmpty=> m_nodeDict.Count == 0;
    }

    public class BundleLetterTree {
        private LetterTreeNode m_head = new LetterTreeNode();

        public void Init(string[] array) {
            for(int i = 0; i < array.Length; i++) {
                Insert(array[i].Replace(Config.bundleExtend,""));
            }
        }

        public void Insert(string str) {
            LetterTreeNode current = m_head;
            LetterTreeNode nextNode = null;
            for(int i = 0; i < str.Length; i++) {
                char letter = str[i];
                if(!current.TryGetNode(letter, out nextNode)) {
                    nextNode = new LetterTreeNode(i == str.Length - 1 ? str : "");
                    current.InsertNode(letter, nextNode);
                }
                current = nextNode;
            }
        }

        public bool TryFind(string assetPath, out string bundlePath, out string assetName) {
            bundlePath = null;
            assetName = null;
            LetterTreeNode current = m_head;
            LetterTreeNode nextNode = null;
            for(int i = 0; i < assetPath.Length; i++) {
                char letter = assetPath[i];
                if(!current.TryGetNode(letter, out nextNode)) {
                    break;
                }
                current = nextNode;
                if(current.Str != null && current.IsChildEmpty) {
                    bundlePath = current.Str;
                    assetName = assetPath.Substring(current.Str.Length+1);
                    return true;
                }else if(current.Str!=null) {
                    // 还有子节点的情况下，再验证一次真的是否存在这个资源
                    bundlePath = current.Str;
                    string depPath = Config.assetPath + current.Str + Config.depExtend;
                    if(File.Exists(depPath)) {
                        assetName = assetPath.Substring(current.Str.Length+1);
                        AssetsBundleInfo info = JsonMapper.ToObject<AssetsBundleInfo>(File.ReadAllText(depPath));
                        if(info != null && info.allAssets.Contains(assetName)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}

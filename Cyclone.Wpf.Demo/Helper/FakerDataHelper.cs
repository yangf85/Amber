using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Demo.Helper;

public class FakerDataHelper
{
    public static List<FakerData> GenerateFakerDataCollection(int count)
    {
        var collection = new List<FakerData>();
        for (int i = 0; i < count; i++)
        {
            collection.Add(new FakerData());
        }
        return collection;
    }

    // 生成 TreeFakerData 随机树形数据集
    public static List<TreeFakerData> GenerateTreeFakerDataCollection(int count)
    {
        var collection = new List<TreeFakerData>();
        for (int i = 0; i < count; i++)
        {
            var node = new TreeFakerData
            {
                Node = Faker.Name.FullName(), // 随机生成节点名称
                Children = GenerateRandomTreeChildren() // 随机生成子节点
            };
            collection.Add(node);
        }
        return collection;
    }

    // 递归生成随机子节点
    private static ObservableCollection<TreeFakerData> GenerateRandomTreeChildren(int maxDepth = 2, int maxChildren = 10)
    {
        var children = new ObservableCollection<TreeFakerData>();
        var random = new Random();

        if (maxDepth > 0)
        {
            int childrenCount = random.Next(1, maxChildren + 1); // 随机生成子节点数量
            for (int i = 0; i < childrenCount; i++)
            {
                var child = new TreeFakerData
                {
                    Node = Faker.Name.FullName(), // 随机生成子节点名称
                    Children = [.. GenerateRandomTreeChildren(maxDepth - 1, maxChildren)] // 递归生成子节点
                };
                children.Add(child);
            }
        }

        return children;
    }
}
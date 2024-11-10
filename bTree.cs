using System.IO;

namespace lr3_data;

using System;
using System.Collections.Generic;

public class BTree
{
    private class BTreeNode
    {
        public List<int> Keys { get; set; }
        public List<string> Values { get; set; }
        public List<BTreeNode> Children { get; set; }
        public bool IsLeaf { get; set; }

        public BTreeNode(bool isLeaf)
        {
            Keys = new List<int>();
            Values = new List<string>();
            Children = new List<BTreeNode>();
            IsLeaf = isLeaf;
        }
    }

    private BTreeNode Root;
    private int T; // Мінімальний поріг (степінь дерева)

    public BTree(int t)
    {
        T = t;
        Root = new BTreeNode(true);
    }

    // Пошук за ключем
    public (string, int) Search(int key)
    {
        return Search(Root, key, 0);
    }

    private (string, int) Search(BTreeNode node, int key, int comparisons)
    {
        int i = 0;
        comparisons++;
        while (i < node.Keys.Count && key > node.Keys[i])
        {
            comparisons++;
            i++;
        }

        comparisons++;
        if (i < node.Keys.Count && key == node.Keys[i])
        {
            return (node.Values[i], comparisons);
        }

        if (node.IsLeaf)
        {
            return (null, comparisons); // Ключ не знайдено
        }

        return Search(node.Children[i], key, comparisons);
    }

    // Вставка елемента в дерево
    public void Insert(int key, string value)
    {
        BTreeNode root = Root;
        if (root.Keys.Count == (2 * T) - 1)
        {
            BTreeNode newRoot = new BTreeNode(false);
            newRoot.Children.Add(Root);
            SplitChild(newRoot, 0);
            Root = newRoot;
        }

        InsertNonFull(Root, key, value);
    }

    private void SplitChild(BTreeNode parent, int index)
    {
        BTreeNode child = parent.Children[index];
        BTreeNode newChild = new BTreeNode(child.IsLeaf);
        int medianIndex = T - 1;

        // Переміщуємо середній елемент до батьківського вузла
        parent.Keys.Insert(index, child.Keys[medianIndex]);
        parent.Values.Insert(index, child.Values[medianIndex]);

        // Розбиваємо дитину
        parent.Children.Insert(index + 1, newChild);

        // Переміщаємо елементи з правої половини дитини в нову дитину
        newChild.Keys.AddRange(child.Keys.GetRange(medianIndex + 1, child.Keys.Count - medianIndex - 1));
        newChild.Values.AddRange(child.Values.GetRange(medianIndex + 1, child.Values.Count - medianIndex - 1));

        child.Keys.RemoveRange(medianIndex, child.Keys.Count - medianIndex);
        child.Values.RemoveRange(medianIndex, child.Values.Count - medianIndex);

        if (!child.IsLeaf)
        {
            newChild.Children.AddRange(child.Children.GetRange(medianIndex + 1, child.Children.Count - medianIndex - 1));
            child.Children.RemoveRange(medianIndex + 1, child.Children.Count - medianIndex - 1);
        }
    }

    private void InsertNonFull(BTreeNode node, int key, string value)
    {
        int i = node.Keys.Count - 1;
        if (node.IsLeaf)
        {
            while (i >= 0 && key < node.Keys[i])
            {
                i--;
            }
            node.Keys.Insert(i + 1, key);
            node.Values.Insert(i + 1, value);
        }
        else
        {
            while (i >= 0 && key < node.Keys[i])
            {
                i--;
            }
            i++;

            if (node.Children[i].Keys.Count == (2 * T) - 1)
            {
                SplitChild(node, i);

                if (key > node.Keys[i])
                {
                    i++;
                }
            }

            InsertNonFull(node.Children[i], key, value);
        }
    }

    public void Delete(int key)
    {
        Delete(Root, key);

        // Якщо корінь порожній і має дочірній вузол, робимо його новим коренем
        if (Root.Keys.Count == 0 && !Root.IsLeaf)
        {
            Root = Root.Children[0];
        }
    }

    private void Delete(BTreeNode node, int key)
    {
        // Знаходимо індекс першого ключа, який більший або дорівнює шуканому
        int index = 0;
        while (index < node.Keys.Count && key > node.Keys[index])
        {
            index++;
        }

        // Випадок 1: ключ знайдено в поточному вузлі
        if (index < node.Keys.Count && node.Keys[index] == key)
        {
            if (node.IsLeaf)
            {
                // Видалення з листка
                node.Keys.RemoveAt(index);
                node.Values.RemoveAt(index);
            }
            else
            {
                // Видалення з внутрішнього вузла
                DeleteFromNonLeaf(node, index);
            }
        }
        else
        {
            // Випадок 2: ключ не знайдено в вузлі, рекурсивно шукаємо у дочірніх вузлах
            if (node.IsLeaf)
            {
                Console.WriteLine("Key not found in the tree.");
                return;
            }

            // Визначаємо, чи є дочірній вузол, який буде оброблений, останнім серед дочірніх
            bool isLastChild = (index == node.Keys.Count);

            // Якщо дочірній вузол має менше, ніж T ключів, заповнюємо його перед рекурсивним викликом
            if (node.Children[index].Keys.Count < T)
            {
                Fill(node, index);
            }

            // Якщо ми заповнюємо останній дочірній вузол після Fill, то може зменшитись кількість ключів,
            // тому викликаємо Delete на оновленому дочірньому вузлі
            if (isLastChild && index > node.Keys.Count)
            {
                Delete(node.Children[index - 1], key);
            }
            else
            {
                Delete(node.Children[index], key);
            }
        }
    }

    private void DeleteFromNonLeaf(BTreeNode node, int index)
    {
        int key = node.Keys[index];

        if (node.Children[index].Keys.Count >= T)
        {
            int predKey = GetPredecessor(node, index);
            string predValue = GetPredecessorValue(node, index);
            node.Keys[index] = predKey;
            node.Values[index] = predValue;
            Delete(node.Children[index], predKey);
        }
        else if (node.Children[index + 1].Keys.Count >= T)
        {
            int succKey = GetSuccessor(node, index);
            string succValue = GetSuccessorValue(node, index);
            node.Keys[index] = succKey;
            node.Values[index] = succValue;
            Delete(node.Children[index + 1], succKey);
        }
        else
        {
            Merge(node, index);
            Delete(node.Children[index], key);
        }
    }

    private int GetPredecessor(BTreeNode node, int index)
    {
        BTreeNode current = node.Children[index];
        while (!current.IsLeaf)
        {
            current = current.Children[current.Keys.Count];
        }
        return current.Keys[^1];
    }

    private string GetPredecessorValue(BTreeNode node, int index)
    {
        BTreeNode current = node.Children[index];
        while (!current.IsLeaf)
        {
            current = current.Children[current.Keys.Count];
        }
        return current.Values[^1];
    }

    private int GetSuccessor(BTreeNode node, int index)
    {
        BTreeNode current = node.Children[index + 1];
        while (!current.IsLeaf)
        {
            current = current.Children[0];
        }
        return current.Keys[0];
    }

    private string GetSuccessorValue(BTreeNode node, int index)
    {
        BTreeNode current = node.Children[index + 1];
        while (!current.IsLeaf)
        {
            current = current.Children[0];
        }
        return current.Values[0];
    }

    private void Fill(BTreeNode node, int index)
    {
        if (index != 0 && node.Children[index - 1].Keys.Count >= T)
        {
            BorrowFromPrev(node, index);
        }
        else if (index != node.Keys.Count && node.Children[index + 1].Keys.Count >= T)
        {
            BorrowFromNext(node, index);
        }
        else
        {
            if (index != node.Keys.Count)
            {
                Merge(node, index);
            }
            else
            {
                Merge(node, index - 1);
            }
        }
    }

    private void BorrowFromPrev(BTreeNode node, int index)
    {
        BTreeNode child = node.Children[index];
        BTreeNode sibling = node.Children[index - 1];

        child.Keys.Insert(0, node.Keys[index - 1]);
        child.Values.Insert(0, node.Values[index - 1]);

        if (!child.IsLeaf)
        {
            child.Children.Insert(0, sibling.Children[^1]);
        }

        node.Keys[index - 1] = sibling.Keys[^1];
        node.Values[index - 1] = sibling.Values[^1];

        sibling.Keys.RemoveAt(sibling.Keys.Count - 1);
        sibling.Values.RemoveAt(sibling.Values.Count - 1);

        if (!sibling.IsLeaf)
        {
            sibling.Children.RemoveAt(sibling.Children.Count - 1);
        }
    }

    private void BorrowFromNext(BTreeNode node, int index)
    {
        BTreeNode child = node.Children[index];
        BTreeNode sibling = node.Children[index + 1];

        child.Keys.Add(node.Keys[index]);
        child.Values.Add(node.Values[index]);

        if (!child.IsLeaf)
        {
            child.Children.Add(sibling.Children[0]);
        }

        node.Keys[index] = sibling.Keys[0];
        node.Values[index] = sibling.Values[0];

        sibling.Keys.RemoveAt(0);
        sibling.Values.RemoveAt(0);

        if (!sibling.IsLeaf)
        {
            sibling.Children.RemoveAt(0);
        }
    }

    private void Merge(BTreeNode node, int index)
    {
        BTreeNode child = node.Children[index];
        BTreeNode sibling = node.Children[index + 1];

        child.Keys.Add(node.Keys[index]);
        child.Values.Add(node.Values[index]);

        child.Keys.AddRange(sibling.Keys);
        child.Values.AddRange(sibling.Values);

        if (!child.IsLeaf)
        {
            child.Children.AddRange(sibling.Children);
        }

        node.Keys.RemoveAt(index);
        node.Values.RemoveAt(index);
        node.Children.RemoveAt(index + 1);
    }
    
    public List<KeyValuePair<int, string>> GetRecords()
    {
        List<KeyValuePair<int, string>> keyValuePairs = new List<KeyValuePair<int, string>>();
        Traverse(Root, keyValuePairs);
        return keyValuePairs;
    }

    private void Traverse(BTreeNode node, List<KeyValuePair<int, string>> keyValuePairs)
    {
        int i;
        for (i = 0; i < node.Keys.Count; i++)
        {
            // Додаємо поточну пару ключ-значення
            keyValuePairs.Add(new KeyValuePair<int, string>(node.Keys[i], node.Values[i]));

            // Рекурсивно обходимо дочірній вузол, якщо він існує
            if (!node.IsLeaf)
            {
                Traverse(node.Children[i], keyValuePairs);
            }
        }

        // Останній дочірній вузол, якщо він є
        if (!node.IsLeaf)
        {
            Traverse(node.Children[i], keyValuePairs);
        }
    }
    
    public void LoadFromFile(string fileName)
    {
        if (File.Exists(fileName))
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    var data = reader.ReadLine().Split(" : ");
                    int key = int.Parse(data[0]);
                    string value = data[1];
                    Insert(key, value); 
                }
            }
        }
    }
    
    public void SaveToFile(string fileName)
    {
        var records = GetRecords();
        using (StreamWriter writer = new StreamWriter(fileName))
        {
            foreach(var record in records)
            {
                writer.WriteLine($"{record.Key} : {record.Value}");
            }
        }
    }
}
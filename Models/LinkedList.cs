namespace ST10159832KeenanPROGpart1.Models
{
    //https://www.youtube.com/watch?v=0AO7OwNzd2Y&t=502s used this video for linked list 
    public class LinkedList
    {
        private class Node
        {
            public IssueViewModel Data { get; set; }
            public Node Next { get; set; }

            public Node(IssueViewModel data)
            {
                Data = data;
                Next = null;
            }
        }

        private Node head;
        private Node tail;
        private int count;

        public LinkedList()
        {
            head = null;
            tail = null;
            count = 0;
        }

        public void Add(IssueViewModel issue)
        {
            Node newNode = new Node(issue);

            if (head == null)
            {
                head = newNode;
                tail = newNode;
            }
            else
            {
                tail.Next = newNode;
                tail = newNode;
            }
            count++;
        }

        public IEnumerable<IssueViewModel> GetAll()
        {
            Node current = head;
            while (current != null)
            {
                yield return current.Data;
                current = current.Next;
            }
        }

        public int Count() => count;
    }
}


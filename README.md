# Unity Infinite Scrolling Custom ListView

## How to use
1. Copy the files into your existing unity project asset folder
2. Attach ```InfiniteScrollingCustomListView.cs``` script to your any object
3. Open the Inspector window and attach ```CustomClass``` to bind to the listview.
4. Design the listview by adding elements.
5. Access ```InfiniteScrollingCustomListView.Instance``` from another script.

### InfiniteScrollingCustomListView.Instance.List
```Gets and sets a list.```
```C#
using UnityEngine;
using System.Collections.Generic;
public class Example : MonoBehaviour
{
    void Start()
    {
        InfiniteScrollingCustomListView.Instance.List = new List<CustomClass>();
        InfiniteScrollingCustomListView.Instance.List.Add(new CustomClass());
    }
}
```
[![Example](https://img.youtube.com/vi/hQgkTqh0YSA/0.jpg)](https://youtu.be/hQgkTqh0YSA)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBasicAssetInfo
{
     string GetName{ get;  }
     string GetLoadingKeyGamePlay { get;}
     string GetLoadingKeyDisplay { get;  }
}

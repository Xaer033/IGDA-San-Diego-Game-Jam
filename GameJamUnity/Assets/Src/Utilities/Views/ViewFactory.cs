﻿using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

namespace GhostGen
{

    public class ViewFactory
    {
        internal class AsyncBlock
        {
            public static AsyncBlock Create(string name, ResourceRequest p_request, OnViewCreated p_callback, Transform p_parent )
            {
                AsyncBlock block = new AsyncBlock();
                block.name = name;
                block.request = p_request;
                block.callback = p_callback;
                block.parent = p_parent;
                return block;
            }

            public ResourceRequest request;
            public OnViewCreated callback;
            public Transform    parent;
            public string name;
       
        }

        public delegate void OnViewCreated(UIView p_view);

        public Canvas canvas { get; set; }

        private List<AsyncBlock> _asyncList = new List<AsyncBlock>();
        private Dictionary<string, UIView> _assetCache = new Dictionary<string, UIView>();
       
        public ViewFactory(Canvas guiCanvas)
        {
            canvas = guiCanvas;
        }   

        public void ClearCache()
        {
            _assetCache.Clear();
        }
   
        public void Step(float deltaTime)
        {
            int asyncLength = _asyncList.Count;
            for(int i = 0; i < asyncLength; ++i)
            { 
                AsyncBlock block = _asyncList[i];
                Assert.IsNotNull(block);
                Assert.IsNotNull(block.request);
            
                if(!block.request.isDone) { continue; }

                Assert.IsNotNull(block.request.asset, "Asset: " + block.name + " couldn't be loaded!");

                UIView prefab = (UIView)block.request.asset;
                _assetCache.Add(block.name, prefab);
                UIView view = _createView(prefab, block.parent);
                Assert.IsNotNull(view);
            
                if(block.callback != null)
                {
                    block.callback(view);
                }

                _asyncList[i] = null;        
            }

            for(int i = asyncLength-1; i >=0; --i)
            {
                if(_asyncList[i] == null)
                {
                    _asyncList.RemoveAt(i);
                }
            }
        }

        public T GetPrefab<T>(string viewPath)
        {
            return (T)(object)Resources.Load<UIView>(viewPath);
        }

        public T Create<T>(string viewPath, Transform parent = null) where T: UIView
        {
            T viewBase = _getPrefab(viewPath) as T;
            Assert.IsNotNull(viewBase);
        
            return (T)(object)_createView(viewBase, parent);
        }

        public T Create<T>(UIView prefab, Transform parent = null)
        {
            Assert.IsNotNull(prefab);
            return (T)(object)_createView(prefab, parent);
        }

        public bool CreateAsync<T>(string viewPath, OnViewCreated callback, Transform parent = null)
        {
            UIView viewPrefab;
            if(_assetCache.TryGetValue(viewPath, out viewPrefab))
            {
                UIView newView = _createView(viewPrefab, parent);
                if(callback != null)
                {
                    callback(newView);
                }
            }
            else
            {
                ResourceRequest request = Resources.LoadAsync<UIView>(viewPath);

                if (request == null) { return false; }
                AsyncBlock block = AsyncBlock.Create(viewPath, request, callback, parent);
                _asyncList.Add(block);
            }

            return true;
        }

        public void RemoveView(UIView view, bool immediately = false)
        {
            Assert.IsNotNull(view);
            if(immediately)
            {
                _removeView(view);
            }
            else
            {
                view.OnViewOutro(()=>
                {
                    _removeView(view);
                });
            }
        }

        private void _removeView(UIView view)
        {
            if(view != null)
            {
                view.OnViewDispose();
                GameObject.Destroy(view.gameObject);
                view = null;
            }
        }

        private UIView _createView(UIView viewBase, Transform parent)
        {
            Transform viewParent = (parent != null) ? parent : canvas.transform;
            Assert.IsNotNull(viewBase);
            UIView view = GameObject.Instantiate<UIView>(viewBase, viewParent, false);
            Singleton.instance.diContainer.InjectGameObjectForComponent<UIView>(view.gameObject);
            return view;
        }

        public UIView _getPrefab(string viewPath)
        {
            UIView view;
            if(!_assetCache.TryGetValue(viewPath, out view))
            {
                view = Resources.Load<UIView>(viewPath);
                _assetCache.Add(viewPath, view);
            }

            return view;
        }

    }
}

﻿//Unity
using UnityEngine;

//C#
using System;

//Game
using Common.Pooling;

namespace Gameplay.Building
{
    public class BuildingSystem : MonoBehaviour
    {
        public BuildConfig _buildConfig;

        public string _boardTag;

        public bool _buildModeEnabled;
        public LayerMask _collisionLayerMask;

        private Camera _mainCam;

        private GameBoard _gameBoard;
        private BuildHologram _buildHologram;

        private const float SEA_LEVEL = 0.5f;
        private const int KINTERVAL = 1;
        private const int KHALFINTERVAL = KINTERVAL / 1;

        private Vector3 seaLevelPos = new Vector3(0, SEA_LEVEL, 0);

        private RaycastHit[] _hitResults = new RaycastHit[5];

        #region Unity Methods
        private void Start()
        {
            _mainCam = Camera.main;
            if(!_mainCam)
            {
                Debug.LogError("NO MAIN CAM FOUND ON BUILDINGSYSTEM");
            }
            _gameBoard = GameObject.FindGameObjectWithTag(_boardTag).GetComponent<GameBoard>();
            if(!_gameBoard)
            {
                Debug.LogError("NO GAME BOARD FOUND ON BUILDINGSYSTEM");
            }
            _buildHologram = GetComponentInChildren<BuildHologram>();
            if(!_buildHologram)
            {
                Debug.LogError("NO BUILD HOLOGRAM FOUND ON BUILDINGSYSTEM");
            }
        }
        private void Update()
        {
            if (_buildModeEnabled)
            {
                //Set our hologram position
                Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
                //IF WE ARE ON THE GAMEBOARD
                if (Physics.RaycastNonAlloc(ray, _hitResults, 100, _gameBoard.GetLayer()) > 0)
                {
                    Vector3 hitPoint = _hitResults[0].point;
                    seaLevelPos.x = hitPoint.x;
                    seaLevelPos.z = hitPoint.z;
                    GridPosition gridPos = GetHoloGramPosition(hitPoint);
                    _buildHologram.SetPosition(gridPos);
                    Debug.DrawRay(hitPoint, Vector3.up, Color.blue);

                    Collider[] overlap = Physics.OverlapBox(_buildHologram.GetPosition(), (_buildHologram.GetScale() / 2.1f), Quaternion.identity, _collisionLayerMask);
                    bool colliding = overlap.Length > 0;
                    _buildHologram.UpdateHologram(colliding, _buildConfig.hologramData);

                    //TRY TO BUILD
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
           
                    }
                }
            }
        }
        #endregion

        #region Hologram position helper
        /// <summary>
        /// Gets the correct Hologram position on the grid
        /// </summary>
        /// <param name="hitPoint"></param>
        /// <returns></returns>
        private GridPosition GetHoloGramPosition(Vector3 hitPoint)
        {
            float boardSize = _gameBoard.GetSize();
            float xOffset = _gameBoard.GetPosition().x;
            float zOffset = _gameBoard.GetPosition().z;

            Vector3 hologramScale = _buildHologram.GetScale();

            float moveByX = (hologramScale.x % 2 == 0) ? 1 : 0.5f;
            float moveByZ = (hologramScale.z % 2 == 0) ? 1 : 0.5f;

            //X
            float x = (hitPoint.x*2);
            x = (float)Math.Round(x, MidpointRounding.AwayFromZero);
            x = x / 2;

            //Y
            float y = SEA_LEVEL;

            //Z
            float z = (hitPoint.z * 2);
            z = (float)Math.Round(z, MidpointRounding.AwayFromZero);
            z = z / 2;

            //RETURN NEW POS
            return GridPosition.Create(x,y,z);

        }
        #endregion
    }
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatSystem : GhostGen.EventDispatcher
{
    private GameplayResources _gameplayResources;
    private GameConfig _gameConfig;
    private GameplayCamera _gameplayCam;
    private CombatCamera _combatCamera;
    private PlayerCombatController _playerCombatController;

    private RaycastHit[] _hitResults = new RaycastHit[5];
    private int _boardLayer;
    private bool _isEnabled;

    public PlayerCombatSystem(
        GameplayResources gameplayResources,
        GameConfig gameConfig)
    {
        _gameplayResources = gameplayResources;
        _gameConfig = gameConfig;

        string[] list = { "BoardLayer" };
        _boardLayer = LayerMask.GetMask(list);

        PlayerCombatView view = GameObject.Instantiate<PlayerCombatView>(_gameplayResources.playerCombatView);
        _playerCombatController = new PlayerCombatController(view, _gameConfig.playerConfig);

        _gameplayCam = GameObject.FindObjectOfType<GameplayCamera>();
        _combatCamera = new CombatCamera(_gameplayCam, _playerCombatController.transform);

        isEnabled = false;
    }

    public bool isEnabled
    {
        get { return _isEnabled; }
        set
        {
            if(_isEnabled != value)
            {
                _isEnabled = value;
                if(_playerCombatController != null)
                {
                    _playerCombatController.isEnabled = value;

                    Singleton.instance.particleGod.GenerateParticle(
                        Gameplay.Particles.Particle.Type.Escape, 
                        _playerCombatController.transform.position);

                    Singleton.instance.audioSystem.PlaySound(Audio.SoundBank.Type.Teleport);
                }

                if(_combatCamera != null)
                {
                    _combatCamera.isEnabled = value;
                }
            }
        }
    }
    
    public PlayerCombatController playerController
    {
        get { return _playerCombatController; }
    }

    public void Tick(float deltaTime)
    {
        if(_playerCombatController != null)
        {
            Vector3 moveDir = getMoveDirection();
            _playerCombatController.SetMoveDirection(moveDir);

            Vector3 aimPos = getAimPosition();
            _playerCombatController.SetAimPosition(aimPos);

            if(Input.GetMouseButton(0))
            {
                _playerCombatController.FireWeapon(aimPos);
            }
            _playerCombatController.Tick(deltaTime);
        }

        if(_combatCamera != null)
        {
            _combatCamera.Tick(deltaTime);
        }
    }

    public void FixedTick(float fixedDeltaTime)
    {
        if(_playerCombatController != null)
        {
            _playerCombatController.FixedTick(fixedDeltaTime);
        }
    }

    private Vector3 getMoveDirection()
    {
        Vector3 result = Vector3.zero;
        if(Input.GetKey(KeyCode.W))
        {
            result += Vector3.forward;
        }
        if(Input.GetKey(KeyCode.S))
        {
            result += Vector3.back;
        }
        if(Input.GetKey(KeyCode.A))
        {
            result += Vector3.left;
        }
        if(Input.GetKey(KeyCode.D))
        {
            result += Vector3.right;
        }
        return result.normalized;
    }

    private Vector3 getAimPosition()
    {
        Vector3 result = Vector3.zero;
        Ray ray = _gameplayCam.camera.ScreenPointToRay(Input.mousePosition);
        //IF WE ARE ON THE GAMEBOARD
        if(Physics.RaycastNonAlloc(ray, _hitResults, 1000, _boardLayer) > 0)
        {
            Vector3 hitPoint = _hitResults[0].point;
            result = hitPoint;

            Debug.DrawRay(hitPoint, Vector3.up, Color.blue);
        }
        return result;
    }
}
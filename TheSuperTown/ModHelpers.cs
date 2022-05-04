using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;
using UnhollowerBaseLib;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using HarmonyLib;
using Hazel;

namespace TheSuperTown { 
    public static class ModHelpers
    {
        public enum MurderAttemptResult
        {
            PerformKill,
            SuppressKill,
            BlankKill,
            GuardianGuardKill
        }
        public static bool ShowButtons
        {
            get
            {
                return !(MapBehaviour.Instance && MapBehaviour.Instance.IsOpen) &&
                      !MeetingHud.Instance &&
                      !ExileController.Instance;
            }
        }
        public static void SetKillTimerUnchecked(this PlayerControl player, float time, float max = float.NegativeInfinity)
        {
            if (max == float.NegativeInfinity) max = time;

            player.killTimer = time;
            DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(time, max);
        }
        public static byte? GetKey(this Dictionary<byte, byte> dec, byte Value)
        {
            foreach (var data in dec)
            {
                if (data.Value == Value)
                {
                    return data.Key;
                }
            }
            return null;
        }// parent直下の子オブジェクトをforeachループで取得する
        public static GameObject[] GetChildren(this GameObject ParentObject)
        {
            GameObject[] ChildObject = new GameObject[ParentObject.transform.childCount];

            for (int i = 0; i < ParentObject.transform.childCount; i++)
            {
                ChildObject[i] = ParentObject.transform.GetChild(i).gameObject;
            }
            return ChildObject;
        }
        public static void DeleteObject(this Transform[] trans, string notdelete)
        {
            foreach (Transform tran in trans)
            {
                if (tran.name != notdelete)
                {
                    GameObject.Destroy(tran);
                }
            }
        }
        public static void DeleteObject(this GameObject[] trans, string notdelete)
        {
            foreach (GameObject tran in trans)
            {
                if (tran.name != notdelete)
                {
                    GameObject.Destroy(tran);
                }
            }
        }
        public static List<PlayerControl> AllNotDisconnectedPlayerControl
        {
            get
            {
                List<PlayerControl> ps = new List<PlayerControl>();
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    if (!p.Data.Disconnected) ps.Add(p);
                }
                return ps;
            }
        }
        public static void SetActiveAllObject(this GameObject[] trans, string notdelete, bool IsActive)
        {
            foreach (GameObject tran in trans)
            {
                if (tran.name != notdelete)
                {
                    tran.SetActive(IsActive);
                }
            }
        }
        public static void setSkinWithAnim(PlayerPhysics playerPhysics, string SkinId)
        {
            SkinViewData nextSkin = DestroyableSingleton<HatManager>.Instance.GetSkinById(SkinId).viewData.viewData;
            AnimationClip clip = null;
            var spriteAnim = playerPhysics.Skin.animator;
            var anim = spriteAnim.m_animator;
            var skinLayer = playerPhysics.Skin;

            var currentPhysicsAnim = playerPhysics.Animator.GetCurrentAnimation();
            if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.RunAnim) clip = nextSkin.RunAnim;
            else if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.SpawnAnim) clip = nextSkin.SpawnAnim;
            else if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.EnterVentAnim) clip = nextSkin.EnterVentAnim;
            else if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.ExitVentAnim) clip = nextSkin.ExitVentAnim;
            else if (currentPhysicsAnim == playerPhysics.CurrentAnimationGroup.IdleAnim) clip = nextSkin.IdleAnim;
            else clip = nextSkin.IdleAnim;

            float progress = playerPhysics.Animator.m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            skinLayer.skin = nextSkin;

            spriteAnim.Play(clip, 1f);
            anim.Play("a", 0, progress % 1);
            anim.Update(0f);
        }
        public static Dictionary<byte, PlayerControl> allPlayersById()
        {
            Dictionary<byte, PlayerControl> res = new Dictionary<byte, PlayerControl>();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                res.Add(player.PlayerId, player);
            return res;
        }

        public static void destroyList<T>(Il2CppSystem.Collections.Generic.List<T> items) where T : UnityEngine.Object
        {
            if (items == null) return;
            foreach (T item in items)
            {
                UnityEngine.Object.Destroy(item);
            }
        }
        public static void destroyList<T>(List<T> items) where T : UnityEngine.Object
        {
            if (items == null) return;
            foreach (T item in items)
            {
                UnityEngine.Object.Destroy(item);
            }
        }
        public static void SetPrivateRole(this PlayerControl player, RoleTypes role, PlayerControl seer = null)
        {
            if (player == null) return;
            if (seer == null) seer = player;
            var clientId = seer.getClientId();
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, (byte)RpcCalls.SetRole, Hazel.SendOption.Reliable, clientId);
            writer.Write((ushort)role);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
        }
        public static InnerNet.ClientData getClient(this PlayerControl player)
        {
            var client = AmongUsClient.Instance.allClients.ToArray().Where(cd => cd.Character.PlayerId == player.PlayerId).FirstOrDefault();
            return client;
        }
        public static int getClientId(this PlayerControl player)
        {
            var client = player.getClient();
            if (client == null) return -1;
            return client.Id;
        }
        public static Sprite loadSpriteFromResources(string path, float pixelsPerUnit)
        {
            try
            {
                Texture2D texture = loadTextureFromResources(path);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            }
            catch
            {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
        }

        public static bool isCustomServer()
        {
            if (DestroyableSingleton<ServerManager>.Instance == null) return false;
            StringNames n = DestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
            return n != StringNames.ServerNA && n != StringNames.ServerEU && n != StringNames.ServerAS;
        }
        public static object TryCast(this Il2CppObjectBase self, Type type)
        {
            return AccessTools.Method(self.GetType(), nameof(Il2CppObjectBase.TryCast)).MakeGenericMethod(type).Invoke(self, Array.Empty<object>());
        }
        internal static string cs(object unityEngine, string v)
        {
            throw new NotImplementedException();
        }

        public static Texture2D loadTextureFromResources(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteTexture = new byte[stream.Length];
                var read = stream.Read(byteTexture, 0, (int)stream.Length);
                LoadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }
            return null;
        }

        public static T GetRandom<T>(List<T> list)
        {
            var indexdate = UnityEngine.Random.Range(0, list.Count);
            return list[indexdate];
        }
        public static PlayerControl GetRandompc(List<PlayerControl> list)
        {
            var indexdate = UnityEngine.Random.Range(0, list.Count);
            return list[indexdate];
        }
        public static int GetRandomIndex<T>(List<T> list)
        {
            var indexdate = UnityEngine.Random.Range(0, list.Count);
            return indexdate;
        }
        public static Texture2D loadTextureFromDisk(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                    byte[] byteTexture = File.ReadAllBytes(path);
                    LoadImage(texture, byteTexture, false);
                    return texture;
                }
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from disk: " + path);
            }
            return null;
        }
        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;
        private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            if (iCall_LoadImage == null)
                iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2cppArray = (Il2CppStructArray<byte>)data;
            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }

        public static PlayerControl playerById(byte id)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == id)
                {
                    return player;
                }
            }
            return null;
        }
        public static bool IsCheckListPlayerControl(this List<PlayerControl> ListDate, PlayerControl CheckPlayer)
        {
            foreach (PlayerControl Player in ListDate)
            {
                if (Player.PlayerId == CheckPlayer.PlayerId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
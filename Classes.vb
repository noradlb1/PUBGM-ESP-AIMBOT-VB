Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading.Tasks
Imports SharpDX
Imports ShpVector3 = SharpDX.Vector3
Imports ShpVector2 = SharpDX.Vector2
Namespace PUBGMESP
	Public Class DisplayData
		Public ViewMatrixBase As Long
		Public myObjectAddress As Long
		Public Players() As PlayerData
		Public Items() As ItemData
		Public Vehicles() As VehicleData
		Public Boxes() As BoxData
		Public Grenades() As GrenadeData

		Public Sub New(ByVal viewMatrixBase As Long, ByVal myObjectAddress As Long)
			Me.ViewMatrixBase = viewMatrixBase
			Me.myObjectAddress = myObjectAddress
		End Sub
	End Class

	Public Class PlayerData
		Public Type As String
		Public Address As Long
		Public Status As Integer
		Public Position As ShpVector3
		Public Pose As Integer
		Public Health As Single
		Public Name As String
		Public IsRobot As Boolean
		Public TeamID As Integer
		Public IsTeam As Integer
	End Class

	Public Class ItemData
		Public Type As Item
		Public Position As ShpVector3
		Public Name As String
	End Class

	Public Class VehicleData
		Public Type As Vehicle
		Public Position As ShpVector3
		Public Name As String
	End Class

	Public Class BoxData
		Public Items() As String
		Public Position As ShpVector3
	End Class

	Public Class GrenadeData
		Public Type As Grenade
		Public Position As ShpVector3
	End Class

	''' <summary>
	''' Item Type
	''' </summary>
	Public Enum Item
		<Description("Useless")> _
		Useless
		<Description("[Med] Enegy Drink")> _
		EnegyDrink
		<Description("[Med] Epinephrine")> _
		Epinephrine
		<Description("[Med] Pain Killer")> _
		PainKiller
		<Description("[Med] First Aid Kit")> _
		AidKit
		<Description("[Armor] Lv.3 Bag")> _
		BagLv3
		<Description("[Armor] Lv.2 Bag")> _
		BagLv2
		<Description("[Armor] Lv.2 Armor")> _
		ArmorLv2
		<Description("[Armor] Lv.3 Armor")> _
		ArmorLv3
		<Description("[Armor] Lv.3 Helmet")> _
		HelmetLv3
		<Description("[Armor] Lv.2 Helmet")> _
		HelmetLv2
		<Description("[Sniper] AWM")> _
		AWM
		<Description("[Rifle] SCAR-L")> _
		SCARL
		<Description("[Sniper] Kar-98")> _
		Kar98
		<Description("[Rifle] M762")> _
		M762
		<Description("[MachineGun] DP-28")> _
		DP28
		<Description("[Rifle] Groza")> _
		Groza
		<Description("[Rifle] AKM")> _
		AKM
		<Description("[Rifle] AUG")> _
		AUG
		<Description("[Rifle] QBZ")> _
		QBZ
		<Description("[MachineGun] M249")> _
		M249
		<Description("[Rifle] M4A1")> _
		M4A1
		<Description("[Ammo] 300 Magnum Ammo")> _
		AmmoMagnum
		<Description("[Ammo] 7.62 Ammo")> _
		Ammo762
		<Description("[Ammo] 5.56 Ammo")> _
		Ammo556
		<Description("[Scope] 4x Scope")> _
		Scope4x
		<Description("[Scope] 6x Scope")> _
		Scope6x
		<Description("[Scope] 8x Scope")> _
		Scope8x
		<Description("[Apendix] Rifle Silenter")> _
		RifleSilenter
		<Description("[Apendix] Rifle Rapid Expansion Magazine")> _
		RifleMagazine
		<Description("[Armor] Ghillie Suit")> _
		GhillieSuit
		<Description("[Pistol] Flare Gun")> _
		FlareGun
		<Description("[Sniper] M24")> _
		M24
		<Description("[Apendix] Sniper Silenter")> _
		SniperSilenter
		<Description("[Sniper] MK14")> _
		MK14
		<Description("[Sniper] SKS")> _
		SKS
		<Description("[Ammo] Grenade")> _
		Grenade
	End Enum

	''' <summary>
	''' Grenade Type
	''' </summary>
	Public Enum Grenade
		<Description("Unknown")> _
		Unknown
		<Description("Smoke Grenade")> _
		Smoke
		<Description("Cocktail Grenade")> _
		Burn
		<Description("Flash Grenade")> _
		Flash
		<Description("Fragment Grenade")> _
		Explode
	End Enum

	''' <summary>
	''' Vehicle Type
	''' </summary>
	Public Enum Vehicle
		<Description("Unknown")> _
		Unknown
		<Description("BRDM")> _
		BRDM
		<Description("Scooter")> _
		Scooter
		<Description("Motorcycle")> _
		Motorcycle
		<Description("MotorcycleCart")> _
		MotorcycleCart
		<Description("Snowmobile")> _
		Snowmobile
		<Description("Tuk")> _
		Tuk
		<Description("Buggy")> _
		Buggy
		<Description("Sports")> _
		Sports
		<Description("Dacia")> _
		Dacia
		<Description("Rony")> _
		Rony
		<Description("PickUp")> _
		PickUp
		<Description("UAZ")> _
		UAZ
		<Description("MiniBus")> _
		MiniBus
		<Description("PG117")> _
		PG117
		<Description("AquaRail")> _
		AquaRail
		<Description("AirPlane")> _
		BP_AirDropPlane_C

	End Enum

	''' <summary>
	''' Aimbot Position
	''' </summary>
	Public Enum AimPosition
		<Description("Head")> _
		Head
		<Description("Chest")> _
		Chest
		<Description("Waist")> _
		Waist
	End Enum
End Namespace

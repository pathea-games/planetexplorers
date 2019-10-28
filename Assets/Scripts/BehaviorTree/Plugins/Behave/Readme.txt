Readme


Installing:

 1: Import Behave.unityPackage.
 2: Create a Behave library from the assets menu.
 3: See documentation (accessible from Help menu).



Upgrading from Behave 2.0:

 1: Move Assets/Behave folder to Assets/Plugins/Behave.
 2: Import package.



Upgrading from Behave 1.x:

NOTE: 0.x assets must first be upgraded by a 1.x install.

 1: Import Behave1xAssetUpgrader.cs.txt from the Behave package and rename it to Behave1xAssetUpgrader.cs.
 2: For each behave library to upgrade:
 	 a: Select the library asset.
 	 b: Export to an external location via the "Assets/Behave/Export asset..." menu.
 3: Delete the existing Behave install - including Behave1xAssetUpgrader.cs.
 4: Import the complete Behave package.
 5: Import the previously exported libraries via the Behave import asset menu.
 6: Verify that the libraries upgraded correctly and delete their old instances.

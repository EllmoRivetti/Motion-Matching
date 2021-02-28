# Motion-Matching

Groupe : PLUSZKIEWICZ Stanislaw, RIVETTI Ellmo, THOMAS Gillian, PEIX Vincent

# Une fois avoir téléchargé et ouvert le projet

Ouvrir la SampleScene

Sur l'objet Character:

- Sur le script Unity Animation Converter, set les fichiers "None" avec les fichiers se trouvant dans le dossier Animations/ et possèdant des icones personalisés.

- Sur le script Animator : double cliquer sur animator_without_merge_issues, cliquer sur la boîte orange 38_04, et assigner dans "Motion" l'animation qui se trouve dans Animations/38_04.

- Lancer la scène, puis appuyer sur le bouton "Load Animation From Unity Animator" dans le script Unity Animation Converter et laisser l'animation se jouer jusqu'à la fin.

- Désactiver les scripts : Animator & Unity Animation Converter

- Activer les scripts : MM_Mover & Animation Controller

- Sur le script Animation Controller, set les fichiers "None" avec les fichiers se trouvant dans le dossier Animations/ et possèdant des icones personalisés.

# Résultat

Au lancement de la scène, le personnage devrait se diriger vers le coffre.

Il est possible de modifier le gameobject "Destination" pendant l'exécution pour forcer des déplacements au personnage.

Il est possible de modifier le nombre de frames jouées avant la réapplication de l'algorithme de motion matching. Cela ne change pas l'écart utilisé pour calculer les positions entre une frame et sa position future.

Le personnage s'arrête une fois qu'il est assez près du coffre, et se redéplace si celui-ci est déplacé.

# Informations supplémentaires

Le lancement du playmode prend quelques secondes à s'exécuter : c'est à ce moment là qu'est effectué le calcul des frames entre elles-mêmes et leur n-ième suivante. Ce n est modifiable dans le fichier Constants.cs, il s'agit de la variable MM_NEXT_FRAME_INTERVAL_SIZE.

Nous avons pris la liberté de laisser les gizmos qui ont servi a débugger pour que vous puissiez voir nos outils de développement.

Nous avons commenté plusieurs méthodes ou marqué comme obsolète des classes qui ne sont pas utilisées/ne fonctionne pas, mais que nous avons préféré garder dans le projet comme "historique" de ce que nous avons essayé.

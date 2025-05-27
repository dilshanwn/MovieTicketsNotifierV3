-- Insert sample movies for development
INSERT INTO movies (title, poster_url, genre, runtime, imdb_rating, synopsis, release_date, status, scope_movie_id) VALUES
(
    'Avatar: The Way of Water',
    'https://image.tmdb.org/t/p/w500/t6HIqrRAclMCA60NsSmeqe9RmNV.jpg',
    'Action, Adventure, Sci-Fi',
    192,
    7.6,
    'Jake Sully lives with his newfound family formed on the extrasolar moon Pandora. Once a familiar threat returns to finish what was previously started, Jake must work with Neytiri and the army of the Na''vi race to protect their planet.',
    '2022-12-16',
    'now_showing',
    'scope_avatar_2'
),
(
    'Top Gun: Maverick',
    'https://image.tmdb.org/t/p/w500/62HCnUTziyWcpDaBO2i1DX17ljH.jpg',
    'Action, Drama',
    130,
    8.3,
    'After thirty years, Maverick is still pushing the envelope as a top naval aviator, but must confront ghosts of his past when he leads TOP GUN''s elite graduates on a mission that demands the ultimate sacrifice from those chosen to fly it.',
    '2022-05-27',
    'now_showing',
    'scope_top_gun'
),
(
    'Black Panther: Wakanda Forever',
    'https://image.tmdb.org/t/p/w500/sv1xJUazXeYqALzczSZ3O6nkH75.jpg',
    'Action, Adventure, Drama',
    161,
    6.7,
    'Queen Ramonda, Shuri, M''Baku, Okoye and the Dora Milaje fight to protect their nation from intervening world powers in the wake of King T''Challa''s death.',
    '2022-11-11',
    'now_showing',
    'scope_black_panther'
),
(
    'Spider-Man: Across the Spider-Verse',
    'https://image.tmdb.org/t/p/w500/8Vt6mWEReuy4Of61Lnj5Xj704m8.jpg',
    'Animation, Action, Adventure',
    140,
    8.4,
    'After reuniting with Gwen Stacy, Brooklyn''s full-time, friendly neighborhood Spider-Man is catapulted across the Multiverse, where he encounters the Spider-Society.',
    '2023-06-02',
    'upcoming',
    'scope_spiderman_verse'
),
(
    'The Flash',
    'https://image.tmdb.org/t/p/w500/rktDFPbfHfUbArZ6OOOKsXcv0Bm.jpg',
    'Action, Adventure, Sci-Fi',
    144,
    6.7,
    'Barry Allen uses his super speed to change the past, but his attempt to save his family creates a world without super heroes, forcing him to race for his life in order to save the future.',
    '2023-06-16',
    'upcoming',
    'scope_flash'
),
(
    'Indiana Jones and the Dial of Destiny',
    'https://image.tmdb.org/t/p/w500/Af4bXE63pVsb2FtbW8uYIyPBadD.jpg',
    'Action, Adventure',
    154,
    6.5,
    'Finding himself in a new era, approaching retirement, Indy wrestles with fitting into a world that seems to have outgrown him.',
    '2023-06-30',
    'upcoming',
    'scope_indiana_jones'
),
(
    'Transformers: Rise of the Beasts',
    'https://image.tmdb.org/t/p/w500/gPbM0MK8CP8A174rmUwGsADNYKD.jpg',
    'Action, Adventure, Sci-Fi',
    127,
    6.0,
    'During the ''90s, a new faction of Transformers - the Maximals - join the Autobots as allies in the battle for Earth.',
    '2023-06-09',
    'upcoming',
    'scope_transformers'
),
(
    'Scream VI',
    'https://image.tmdb.org/t/p/w500/wDWwtvkRRlgTiUr6TyLSMX8FCuZ.jpg',
    'Horror, Mystery, Thriller',
    123,
    6.5,
    'Following the latest Ghostface killings, the four survivors leave Woodsboro behind and start a fresh chapter.',
    '2023-03-10',
    'now_showing',
    'scope_scream_6'
);

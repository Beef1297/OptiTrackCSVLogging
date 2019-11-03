# CSV Log

- とりあえず 個人リポジトリで，いまいち扱いが分からないので

## Scripts

- MyScripts
  - OptiCSVLogging.cs : OptiTrack のデータを csv で書き出す
	  - 今は FixedUpdate で記録
		  - 100FPS
		- Controller を用意して startLogging(), endLogging() を呼ぶ
		- 書き出す時は writeCSV()
    - TODO: スレッド化, 汎用的な csv 書き出しを作成
  - その他
    - 自分の実験で使うやつ

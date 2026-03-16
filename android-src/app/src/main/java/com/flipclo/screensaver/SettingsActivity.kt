package com.fliqlo.screensaver

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import androidx.preference.CheckBoxPreference
import androidx.preference.PreferenceCategory
import androidx.preference.PreferenceFragmentCompat
import androidx.preference.SeekBarPreference

/**
 * Settings activity launched from the system screen saver picker.
 */
class SettingsActivity : AppCompatActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        if (savedInstanceState == null) {
            supportFragmentManager
                .beginTransaction()
                .replace(android.R.id.content, SettingsFragment())
                .commit()
        }
    }

    class SettingsFragment : PreferenceFragmentCompat() {
        override fun onCreatePreferences(savedInstanceState: Bundle?, rootKey: String?) {
            val screen = preferenceManager.createPreferenceScreen(requireContext())

            screen.addPreference(CheckBoxPreference(requireContext()).apply {
                key = "use_24_hour"
                title = getString(R.string.pref_use_24_hour)
                setDefaultValue(true)
            })

            screen.addPreference(CheckBoxPreference(requireContext()).apply {
                key = "show_seconds"
                title = getString(R.string.pref_show_seconds)
                setDefaultValue(false)
            })

            // Scale category
            val scaleCategory = PreferenceCategory(requireContext()).apply {
                title = getString(R.string.pref_scale_category)
            }
            screen.addPreference(scaleCategory)

            scaleCategory.addPreference(SeekBarPreference(requireContext()).apply {
                key = "horizontal_scale"
                title = getString(R.string.pref_horizontal_scale)
                min = 50
                max = 200
                setDefaultValue(100)
                showSeekBarValue = true
            })

            scaleCategory.addPreference(SeekBarPreference(requireContext()).apply {
                key = "vertical_scale"
                title = getString(R.string.pref_vertical_scale)
                min = 50
                max = 200
                setDefaultValue(100)
                showSeekBarValue = true
            })

            scaleCategory.addPreference(SeekBarPreference(requireContext()).apply {
                key = "overall_scale"
                title = getString(R.string.pref_overall_scale)
                min = 50
                max = 200
                setDefaultValue(100)
                showSeekBarValue = true
            })

            preferenceScreen = screen
        }
    }
}

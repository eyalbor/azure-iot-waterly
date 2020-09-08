import { combineReducers } from 'redux'
import { reducer as formReducer } from 'redux-form'
import authReducer from './authReducer';
import deviceReducer from './deviceReducer'
import eventReducer from './eventReducer'
import notificationsReducer from './notificationsReducer'
import billsReducer from './billReducer'
import qualityReducer from './qualityReducer'


export default combineReducers({
    auth: authReducer,
    form: formReducer,
    devices: deviceReducer,
    events: eventReducer,
    notifications: notificationsReducer,
    bills: billsReducer,
    quality: qualityReducer
});
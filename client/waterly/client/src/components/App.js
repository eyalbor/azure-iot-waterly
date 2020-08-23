import React from 'react';
import { Router, Route, Switch } from 'react-router-dom'
import DeviceCreate from './devices/DeviceCreate'
import DeviceDelete from './devices/DeviceDelete'
import DeviceEdit from './devices/DeviceEdit'
import DeviceList from './devices/DeviceList'
import EventsList from './events/EventsList'
import BillShow from './bill/BillShow'
import Header from './Header';
import HomePage from './HomePage'
import Notfound from './NoFound'
import history from '../history'
import ProtectedRoute from '../actions/ProtectedRoute'
import Notifications from './notifications/NotificationPage';

const App = () => {
  return (
    <div>
      <Router history={history}>
          <Header/>
            <Switch>
              <Route path="/" exact component={HomePage}/>
              <ProtectedRoute path="/devices/list" exact component={DeviceList}/>
              <ProtectedRoute path="/bill/show" exact component={BillShow}/>
              <ProtectedRoute path="/devices/new" exact component={DeviceCreate}/>
              <ProtectedRoute path="/devices/edit/:id" exact component={DeviceEdit}/>
              <ProtectedRoute path="/devices/delete" exact component={DeviceDelete}/>
              <Route path="/events/device/:device_id" exact component={EventsList}/>
              <Route path="/notifications" exact component={Notifications}/>
              <Route component={Notfound} />
            </Switch>
      </Router>
    </div>
  );
};

export default App;

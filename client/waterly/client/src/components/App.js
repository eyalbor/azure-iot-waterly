import React from 'react';
import { Router, Route, Redirect } from 'react-router-dom'
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

const App = () => {
  return (
    <div>
      <Router history={history}>
          <Header/>
            <div>
              <Route path="/" exact component={HomePage}/>
              <Route path="/devices/list" exact component={DeviceList}/>
              <Route path="/bill/show" exact component={BillShow}/>
              <Route path="/devices/new" exact component={DeviceCreate}/>
              <Route path="/devices/edit/:id" exact component={DeviceEdit}/>
              <Route path="/devices/delete" exact component={DeviceDelete}/>
              <Route path="/events/:id" exact component={EventsList}/>
              <Route path="/404" component={Notfound} />
              <Route path="*" render={() => <Redirect to="/404" />} />
            </div>
      </Router>
    </div>
  );
};

export default App;
